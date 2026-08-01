// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Framework.Streaming
{
	/// <summary>
	/// Flat asset-name -> path lookup table, written once by a baker and read once at runtime.
	/// Deliberately not JSON/MessagePack: for a flat string-&gt;string map with no nesting and no
	/// versioning needs beyond a single format tag, a sequential length-prefixed read/write is
	/// both the simplest and the fastest thing to produce and to parse - .NET's own
	/// BinaryWriter/BinaryReader string methods already do exactly that length-prefixing
	/// (7-bit-encoded length + UTF8 bytes) in tight, allocation-light framework code, so there's
	/// nothing meaningfully faster to hand-rewrite here.
	///
	/// Takes a Stream, not a path - Core never opens a file itself (see IChunkDataSource for why
	/// that split matters): a raw filesystem path isn't valid once assets are packed into a
	/// Godot .pck, so the glue layer is the one that knows how to actually get bytes for a given
	/// url (Godot's FileAccess) and hands this class a stream over them.
	/// </summary>
	public static class AssetManifest
	{
		private const int FormatVersion = 1;

		public static void Save( Stream stream, IReadOnlyDictionary<string, string> entries )
		{
			using var writer = new BinaryWriter( stream, Encoding.UTF8, leaveOpen: true );
			writer.Write( FormatVersion );
			writer.Write( entries.Count );
			foreach ( KeyValuePair<string, string> entry in entries )
			{
				writer.Write( entry.Key );
				writer.Write( entry.Value );
			}
		}

		public static Dictionary<string, string> Load( Stream stream )
		{
			using var reader = new BinaryReader( stream, Encoding.UTF8, leaveOpen: true );
			int version = reader.ReadInt32();
			if ( version != FormatVersion )
				throw new InvalidDataException( $"Asset manifest is format version {version}; this build only understands version {FormatVersion}." );

			int count = reader.ReadInt32();
			var result = new Dictionary<string, string>( count, StringComparer.Ordinal );
			for ( int i = 0; i < count; i++ )
			{
				string key = reader.ReadString();
				string value = reader.ReadString();
				result[ key ] = value;
			}

			return result;
		}
	}
}
