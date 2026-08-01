// Created by Anton Piruev in 2026.
// Texture Baker is a sandalone utility released under MIT license

using Framework.Streaming;
using System;
using System.Collections.Generic;
using System.IO;

namespace TextureBaker
{
	/// <summary>
	/// Manifest I/O for the baker side. Unlike the runtime's AssetManifestService (which must go
	/// through Godot's FileAccess because res:// can be inside a packed .pck), this tool is a
	/// plain .NET process working with real files on disk, so straightforward System.IO is fine
	/// here - only the *content* format (AssetManifest.Save/Load) needs to match the runtime.
	/// </summary>
	internal static class ManifestTool
	{
		public static Dictionary<string, string> LoadOrEmpty( string path )
		{
			if ( !File.Exists( path ) )
				return new Dictionary<string, string>( StringComparer.Ordinal );

			using FileStream stream = File.OpenRead( path );
			return AssetManifest.Load( stream );
		}

		public static void Save( string path, IReadOnlyDictionary<string, string> entries )
		{
			string? directory = Path.GetDirectoryName( path );
			if ( !string.IsNullOrEmpty( directory ) )
				Directory.CreateDirectory( directory );

			using FileStream stream = File.Create( path );
			AssetManifest.Save( stream, entries );
		}

		/// <summary>
		/// Walks upward from <paramref name="absolutePath"/> looking for a project.godot, and if
		/// found, returns the equivalent "res://..." path. Bounded to 12 levels so a path outside
		/// any Godot project doesn't turn into an unbounded directory walk up to the drive root.
		/// </summary>
		public static bool TryToResPath( string absolutePath, out string resPath )
		{
			string? dir = Path.GetDirectoryName( Path.GetFullPath( absolutePath ) );
			for ( int i = 0; i < 12 && dir != null; i++ )
			{
				if ( File.Exists( Path.Combine( dir, "project.godot" ) ) )
				{
					string relative = Path.GetRelativePath( dir, absolutePath ).Replace( '\\', '/' );
					resPath = "res://" + relative;
					return true;
				}

				dir = Path.GetDirectoryName( dir );
			}

			resPath = string.Empty;
			return false;
		}
	}
}
