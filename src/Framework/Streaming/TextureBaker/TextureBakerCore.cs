// Created by Anton Piruev in 2026.
// Texture Baker is a sandalone utility released under MIT license

using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using Framework.Streaming;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextureBaker
{
	/// <summary>Which compressed format to bake to. Auto picks per-texture based on whether the source actually uses its alpha channel.</summary>
	public enum BakeFormat
	{
		Auto,
		Bc1,
		Bc3,
	}

	public readonly struct BakeResult
	{
		public readonly string OutputPath;
		public readonly int MipCount;
		public readonly long TotalBytes;
		public readonly CompressionFormat Format;

		public BakeResult( string outputPath, int mipCount, long totalBytes, CompressionFormat format )
		{
			OutputPath = outputPath;
			MipCount = mipCount;
			TotalBytes = totalBytes;
			Format = format;
		}
	}

	/// <summary>
	/// Mirrors the handful of Godot.Image.Format values this baker actually produces (see
	/// core/io/image.h in Godot's source - FORMAT_DXT1 = 17, FORMAT_DXT5 = 19). Kept as a local
	/// constant rather than referencing GodotSharp from this standalone tool, since this project
	/// deliberately has zero dependency on the Godot runtime - only on the exact same
	/// Streaming.Integration container format the runtime addon reads.
	/// </summary>
	internal static class GodotImageFormat
	{
		public const int Dxt1 = 17;
		public const int Dxt5 = 19;
	}

	/// <summary>
	/// BC1 (opaque) / BC3 (alpha) - the S3TC family - is the one compressed format guaranteed to
	/// work identically on both Godot's Compatibility (GLES3) and Vulkan-mobile/Vulkan renderers,
	/// on both Windows and Linux, without extra per-platform baking: it's been standard desktop
	/// GPU hardware since the early 2000s and every desktop OpenGL/Vulkan driver decodes it
	/// natively. ETC2 is the mobile-oriented alternative Godot uses instead on Android/iOS/web -
	/// not relevant for a desktop FPS. BC7 gives better quality but needs a DX10 DDS header and
	/// wider hardware feature support to guarantee - deliberately left out of this baker to keep
	/// the "just works everywhere this project ships" guarantee simple.
	/// </summary>
	public static class TextureBakerCore
	{
		public static BakeResult Bake( string inputPath, string outputPath, BakeFormat requestedFormat )
		{
			using Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>( inputPath );

			CompressionFormat format = requestedFormat switch
			{
				BakeFormat.Bc1 => CompressionFormat.Bc1,
				BakeFormat.Bc3 => CompressionFormat.Bc3,
				_ => HasAlpha( image ) ? CompressionFormat.Bc3 : CompressionFormat.Bc1,
			};

			var encoder = new BcEncoder();
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Quality = CompressionQuality.Balanced;
			encoder.OutputOptions.Format = format;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var ddsStream = new MemoryStream();
			encoder.EncodeToStream( image, ddsStream );

			List<(int Width, int Height, byte[] Bytes)> mipsLargestFirst = DdsMipReader.ReadMips( ddsStream.ToArray(), format );
			int godotFormat = format == CompressionFormat.Bc1 ? GodotImageFormat.Dxt1 : GodotImageFormat.Dxt5;

			// Core numbers residency 0 = smallest mip (see StreamableResource/GodotStreamableTexture) -
			// DDS gives us largest-first, so we reverse while building each self-describing chunk.
			var chunksSmallestFirst = new List<byte[]>( mipsLargestFirst.Count );
			for ( int i = mipsLargestFirst.Count - 1; i >= 0; i-- )
			{
				(int Width, int Height, byte[] Bytes) mip = mipsLargestFirst[ i ];

				using var chunkStream = new MemoryStream();
				using ( var writer = new BinaryWriter( chunkStream, System.Text.Encoding.UTF8, leaveOpen: true ) )
				{
					writer.Write( mip.Width );
					writer.Write( mip.Height );
					writer.Write( godotFormat );
					writer.Write( mip.Bytes );
				}

				chunksSmallestFirst.Add( chunkStream.ToArray() );
			}

			string? directory = Path.GetDirectoryName( outputPath );
			if ( !string.IsNullOrEmpty( directory ) )
				Directory.CreateDirectory( directory );

			ChunkStorage.Pack( outputPath, chunksSmallestFirst );

			long totalBytes = chunksSmallestFirst.Sum( c => (long)c.Length );
			return new BakeResult( outputPath, chunksSmallestFirst.Count, totalBytes, format );
		}

		private static bool HasAlpha( Image<Rgba32> image )
		{
			bool hasAlpha = false;
			image.ProcessPixelRows( accessor =>
			{
				for ( int y = 0; y < accessor.Height && !hasAlpha; y++ )
				{
					Span<Rgba32> row = accessor.GetRowSpan( y );
					for ( int x = 0; x < row.Length; x++ )
					{
						if ( row[ x ].A != 255 )
						{
							hasAlpha = true;
							break;
						}
					}
				}
			} );
			return hasAlpha;
		}
	}
}
