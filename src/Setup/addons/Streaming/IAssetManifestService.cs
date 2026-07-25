// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Setup.addons.Streaming
{
    /// <summary>
    /// Resolves a baked asset's bare name (e.g. "wall_albedo") to its full Godot path (e.g.
    /// "res://Cook/wall_albedo.stream"). Backed by a manifest baked alongside the .stream files
    /// themselves - see AssetManifestService and TextureBaker's --manifest option.
    /// </summary>
    public interface IAssetManifestService
    {
        /// <summary>
        /// Looks up <paramref name="assetName"/>. Throws if it isn't in the manifest - a
        /// streamable node referencing a name that was never baked is a build/content error,
        /// not something to silently paper over with a null and a missing texture in-game.
        /// </summary>
        string Resolve( string assetName );
    }
}
