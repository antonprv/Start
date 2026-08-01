// Created by Anton Piruev in 2026.
// Texture Baker is a sandalone utility released under MIT license

using System;
using System.IO;

namespace TextureBaker
{
	/// <summary>
	/// The command-line side of the tool - what a CI/CD pipeline actually shells out to.
	/// Exit code 0 means every requested texture baked cleanly; nonzero means at least one
	/// failed, so a pipeline step can fail the build on a bad source asset instead of silently
	/// shipping a stale or missing .stream file.
	/// </summary>
	internal static class CliBaker
	{
		private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".tga", ".bmp" };

		public static int Run( string[] args )
		{
			if ( args.Length == 0 || args[ 0 ] is "-h" or "--help" or "help" )
			{
				PrintUsage();
				return args.Length == 0 ? 1 : 0;
			}

			string? input = null;
			string? output = null;
			string? manifestPath = null;
			string? resPrefix = null;
			var format = BakeFormat.Auto;
			bool recursive = true;

			foreach ( string arg in args )
			{
				if ( arg.StartsWith( "--input=", StringComparison.OrdinalIgnoreCase ) )
					input = arg[ "--input=".Length.. ];
				else if ( arg.StartsWith( "--output=", StringComparison.OrdinalIgnoreCase ) )
					output = arg[ "--output=".Length.. ];
				else if ( arg.StartsWith( "--format=", StringComparison.OrdinalIgnoreCase ) )
					format = ParseFormat( arg[ "--format=".Length.. ] );
				else if ( arg.StartsWith( "--manifest=", StringComparison.OrdinalIgnoreCase ) )
					manifestPath = arg[ "--manifest=".Length.. ];
				else if ( arg.StartsWith( "--res-prefix=", StringComparison.OrdinalIgnoreCase ) )
					resPrefix = arg[ "--res-prefix=".Length.. ];
				else if ( string.Equals( arg, "--no-recursive", StringComparison.OrdinalIgnoreCase ) )
					recursive = false;
			}

			if ( input == null || output == null )
			{
				Console.Error.WriteLine( "Both --input=<path> and --output=<path> are required. Run with --help for usage." );
				return 1;
			}

			if ( Directory.Exists( input ) )
				return BakeDirectory( input, output, format, recursive, manifestPath, resPrefix ?? "res://Cook/" );

			try
			{
				BakeResult result = TextureBakerCore.Bake( input, output, format );
				Console.WriteLine( $"OK  {input} -> {result.OutputPath}  ({result.Format}, {result.MipCount} mips, {result.TotalBytes} bytes)" );
				return 0;
			}
			catch ( Exception ex )
			{
				Console.Error.WriteLine( $"FAIL {input}: {ex.Message}" );
				return 1;
			}
		}

		private static int BakeDirectory( string inputDir, string outputDir, BakeFormat format, bool recursive, string? manifestPath, string resPrefix )
		{
			SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			int failures = 0;
			int count = 0;
			var manifest = new System.Collections.Generic.Dictionary<string, string>( StringComparer.Ordinal );

			foreach ( string file in Directory.EnumerateFiles( inputDir, "*.*", searchOption ) )
			{
				if ( Array.IndexOf( SupportedExtensions, Path.GetExtension( file ).ToLowerInvariant() ) < 0 )
					continue;

				string relative = Path.GetRelativePath( inputDir, file );
				string relativeStream = Path.ChangeExtension( relative, ".stream" );
				string outputPath = Path.Combine( outputDir, relativeStream );
				string assetName = Path.GetFileNameWithoutExtension( relative );

				if ( manifestPath != null && manifest.TryGetValue( assetName, out string? existingPath ) )
				{
					// A flat name -> path manifest only works if names are unique project-wide -
					// that's the whole point of keeping StreamableTexture2D's AssetName simple.
					// Two source textures sharing a bare filename in different folders would
					// otherwise silently overwrite each other's manifest entry, and whichever one
					// "wins" would depend on directory enumeration order - a build-time failure
					// here is much cheaper to fix than that bug is to track down later.
					Console.Error.WriteLine( $"FAIL {relative}: asset name '{assetName}' collides with an existing entry ({existingPath}) - rename one of the two source files." );
					failures++;
					continue;
				}

				try
				{
					BakeResult result = TextureBakerCore.Bake( file, outputPath, format );
					Console.WriteLine( $"OK  {relative}  ({result.Format}, {result.MipCount} mips, {result.TotalBytes} bytes)" );
					count++;

					if ( manifestPath != null )
						manifest[ assetName ] = resPrefix.TrimEnd( '/' ) + "/" + relativeStream.Replace( '\\', '/' );
				}
				catch ( Exception ex )
				{
					Console.Error.WriteLine( $"FAIL {relative}: {ex.Message}" );
					failures++;
				}
			}

			if ( manifestPath != null && failures == 0 )
			{
				// Full rebuild, not a merge with whatever was there before: a texture that gets
				// deleted or renamed from the source tree should disappear from the manifest too,
				// rather than leaving a dangling entry pointing at a .stream file nothing
				// regenerates anymore. If you bake different subfolders through separate
				// invocations of this tool, point --manifest at a different file per invocation,
				// or bake through one --input covering everything you want in one manifest.
				ManifestTool.Save( manifestPath, manifest );
				Console.WriteLine( $"Wrote {manifest.Count} entries to {manifestPath}." );
			}

			Console.WriteLine( $"Baked {count} texture(s), {failures} failure(s)." );
			return failures == 0 ? 0 : 1;
		}

		private static BakeFormat ParseFormat( string value ) => value.ToLowerInvariant() switch
		{
			"bc1" => BakeFormat.Bc1,
			"bc3" => BakeFormat.Bc3,
			_ => BakeFormat.Auto,
		};

		private static void PrintUsage()
		{
			Console.WriteLine( "TextureBaker - bakes textures into the chunked .stream format read by StreamableTexture2D." );
			Console.WriteLine();
			Console.WriteLine( "Usage:" );
			Console.WriteLine( "  TextureBaker.exe --input=<file-or-dir> --output=<file-or-dir> [--format=auto|bc1|bc3] [--no-recursive]" );
			Console.WriteLine( "                    [--manifest=<path>] [--res-prefix=res://Cook/]   (directory mode only)" );
			Console.WriteLine( "  TextureBaker.exe app                                            Launches the GUI instead of the CLI." );
			Console.WriteLine();
			Console.WriteLine( "Examples:" );
			Console.WriteLine( "  TextureBaker.exe --input=Art/wall.png --output=Streamed/wall.stream" );
			Console.WriteLine( "  TextureBaker.exe --input=Art/Textures --output=Cook --manifest=Cook/manifest.bin --res-prefix=res://Cook/" );
			Console.WriteLine( "                    (bakes every texture found, preserving folder structure, and (re)writes the name -> res:// path manifest StreamableTexture2D resolves AssetName against)" );
		}
	}
}
