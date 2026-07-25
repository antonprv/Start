// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;
using System.IO;
using Framework.Logger;
using Framework.Streaming;

using GFile = Godot.FileAccess;
namespace Setup.addons.Streaming
{
    /// <summary>
    /// Reads Cook/manifest.bin through Godot's FileAccess - not System.IO - the very first time
    /// something asks it to resolve a name, then serves every later Resolve() call out of the
    /// resulting Dictionary with no further disk I/O or re-parsing. Register one instance of this
    /// as a singleton in Zenjex (same pattern as IStreamingWorld/StreamingWorldNode) so every
    /// StreamableTexture2D/StreamableMeshInstance3D in the game shares the same loaded manifest
    /// instead of each parsing its own copy.
    ///
    /// System.IO.File would only work while running loose files from the editor - once the
    /// project is exported, res:// paths live inside a .pck archive and plain filesystem APIs
    /// can't see them at all. Routing the read through Godot's own FileAccess is what makes this
    /// work identically in the editor and in a shipped build.
    /// </summary>
    public sealed class AssetManifestService : IAssetManifestService
    {
        private readonly string _manifestPath;
        private Dictionary<string, string>? _entries;

        public AssetManifestService( string manifestPath = "res://Cook/manifest.bin" )
        {
            _manifestPath = manifestPath;
        }

        /// <summary>
        /// Forces the load now rather than on the first Resolve() call - call this once during a
        /// loading screen if you'd rather pay the (small, but non-zero for a few thousand
        /// entries) parse cost there instead of on whatever frame the first streamable resource
        /// happens to register.
        /// </summary>
        public void EnsureLoaded()
        {
            if ( _entries != null )
                return;

            using GFile file = GFile.Open( _manifestPath, GFile.ModeFlags.Read );
            if ( file == null )
            {
                GameLogger.LogError( $"AssetManifest: couldn't open '{_manifestPath}' ({GFile.GetOpenError()}) - no streamable asset names will resolve until this exists." );
                _entries = new Dictionary<string, string>();
                return;
            }

            byte[] bytes = file.GetBuffer( (long)file.GetLength() );
            using var stream = new MemoryStream( bytes );
            _entries = AssetManifest.Load( stream );

            GameLogger.LogInfo( $"AssetManifest: loaded {_entries.Count} entries from {_manifestPath}." );
        }

        public string Resolve( string assetName )
        {
            EnsureLoaded();

            if ( _entries!.TryGetValue( assetName, out string? path ) )
                return path;

            throw new KeyNotFoundException( $"AssetManifest: no entry for '{assetName}' - was it baked into Cook/ before this build?" );
        }
    }
}
