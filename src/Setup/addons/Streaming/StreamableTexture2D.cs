// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using Framework.Logger;
using Framework.Streaming;
using Godot;
using Setup.addons.Streaming;
using Zenjex;

namespace Streaming
{
    /// <summary>
    /// A texture whose mips stream in as the owning node gets closer to the camera.
    ///
    /// Each chunk is one mip level packed as [int32 width][int32 height][int32 godotFormat][raw
    /// pixel/block bytes for that level] - raw bytes in an Image.Format Godot already understands
    /// (Rgba8, Dxt1/Bc1, Bc7, ...), not PNG. Godot's own Image class can produce this at bake time
    /// via GenerateMipmaps() + Compress() - the baker doesn't need an external texture compressor,
    /// just a small editor/CLI script that runs those two calls per source image and slices the
    /// result into per-mip chunks.
    ///
    /// Residency level here is "how many of the smallest mips we have", 0 = smallest only, higher
    /// = more detail - the opposite direction from how Godot's own in-progress texture streaming
    /// fork numbers mips (there, mip 0 is full res and the number increases toward the smallest).
    /// Neither is wrong, just be aware of the flip if cross-referencing that code.
    ///
    /// Applies each residency change as a full ImageTexture replacement (a new Image built from
    /// every currently-loaded mip, largest first, via Image.CreateFromData) rather than a partial
    /// GPU sub-resource update. This mirrors what Godot's own ImageTexture.SetImage and the
    /// in-progress engine-level texture streaming module both do internally - texture_2d_create
    /// plus texture_replace under the hood - so it's not a shortcut relative to what "real" Godot
    /// streaming does, it's the same strategy. The cost that comes with it: every residency step
    /// re-uploads the *entire* currently-resident mip range, not just the newly streamed-in top
    /// mip, since Godot's public texture API has no cheaper "just add one more mip" path.
    ///
    /// AssetName (not a raw path) is what you set in the inspector - it's resolved to the actual
    /// .stream path via IAssetManifestService exactly once, in _Ready. That lookup is a plain
    /// Dictionary hit against an already-in-memory table (see AssetManifestService), not disk
    /// I/O, so doing it once per node instantiation - i.e. once per level load, for every
    /// streamable object the level contains - costs nothing worth avoiding further.
    /// </summary>
    [GlobalClass]
    public partial class StreamableTexture2D : Node3D
    {
        [ExportGroup( "Source" )]
        [Export] private string _assetName = string.Empty;

        [Signal]
        public delegate void TextureUpdatedEventHandler( Texture2D texture );

        public Texture2D? CurrentTexture { get; private set; }

        private IStreamingWorld _streamingWorld = null!;
        private IAssetManifestService _manifest = null!;
        private GodotStreamableTexture? _resource;

        [Inject]
        private void Construct( IStreamingWorld world, IAssetManifestService manifest )
        {
            _streamingWorld = world;
            _manifest = manifest;
        }

        public override void _EnterTree() => DiContainer.Instance.Inject( this );

        public override void _Ready()
        {
            if ( string.IsNullOrEmpty( _assetName ) )
            {
                GameLogger.LogError( $"{Name}: StreamableTexture2D requires an AssetName." );
                return;
            }

            string url;
            try
            {
                url = _manifest.Resolve( _assetName );
            }
            catch ( KeyNotFoundException ex )
            {
                GameLogger.LogError( $"{Name}: {ex.Message}" );
                return;
            }

            _resource = new GodotStreamableTexture(
                _streamingWorld.Core,
                url,
                new System.Numerics.Vector3( GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z ),
                OnTextureReplaced );
        }

        public override void _ExitTree() => _resource?.Unregister();

        public override void _Process( double delta )
        {
            if ( _resource != null )
                _resource.WorldPosition = new System.Numerics.Vector3( GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z );
        }

        private void OnTextureReplaced( Texture2D texture )
        {
            CurrentTexture = texture;
            EmitSignal( SignalName.TextureUpdated, texture );
        }
    }

    /// <summary>One mip level's decoded header + raw pixel bytes, cached so a residency change never needs to touch levels it isn't replacing.</summary>
    internal readonly struct DecodedMipLevel
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Image.Format Format;
        public readonly byte[] Bytes;

        public DecodedMipLevel( int width, int height, Image.Format format, byte[] bytes )
        {
            Width = width;
            Height = height;
            Format = format;
            Bytes = bytes;
        }
    }

    /// <summary>
    /// Core-facing half of <see cref="StreamableTexture2D"/> - the actual StreamableResource
    /// subclass. Split out so the Godot Node stays a thin wrapper and this class can be unit
    /// tested (mip decode + residency math) without booting the Godot runtime.
    /// </summary>
    internal sealed class GodotStreamableTexture : StreamableResource
    {
        private readonly Action<Texture2D> _onTextureReplaced;
        // Kept independently of ChunkStorage's own cache: Core is free to release a chunk's raw
        // bytes once every dependent level has been applied, but we still need each level's
        // decoded header to rebuild the pyramid image on the *next* residency change.
        private readonly Dictionary<int, DecodedMipLevel> _decodedLevels = new();
        private ImageTexture? _texture;

        public GodotStreamableTexture(
            StreamingWorld world,
            string url,
            System.Numerics.Vector3 worldPosition,
            Action<Texture2D> onTextureReplaced )
            : base( world, url, StreamableKind.Texture, worldPosition )
        {
            _onTextureReplaced = onTextureReplaced;
        }

        protected override void OnFirstResidency()
        {
            _texture = new ImageTexture();
        }

        /// <summary>Background thread: just splits the chunk into (width, height, format, bytes) - no Godot object is touched here.</summary>
        protected override object? PrepareChunk( int level, ReadOnlyMemory<byte> chunkData )
        {
            ReadOnlySpan<byte> span = chunkData.Span;
            if ( span.Length < 12 )
            {
                GameLogger.LogError( $"Streaming: mip {level} of '{Handle}' is smaller than its own header." );
                return null;
            }

            int width = BitConverter.ToInt32( span.Slice( 0, 4 ) );
            int height = BitConverter.ToInt32( span.Slice( 4, 4 ) );
            var format = (Image.Format)BitConverter.ToInt32( span.Slice( 8, 4 ) );
            byte[] bytes = span.Slice( 12 ).ToArray();

            return new DecodedMipLevel( width, height, format, bytes );
        }

        /// <summary>
        /// Main thread: records the newly decoded level, then rebuilds one Image spanning every
        /// resident level from <paramref name="level"/> (the largest we currently have) down to
        /// the smallest, and swaps it into the texture in one call - a partial pyramid is exactly
        /// as valid an Image as a complete one, Godot just treats it as a texture with fewer mips.
        /// </summary>
        protected override void ApplyPreparedChunk( int level, object? prepared )
        {
            if ( prepared is not DecodedMipLevel decoded || _texture == null )
                return;

            _decodedLevels[ level ] = decoded;

            int totalBytes = 0;
            for ( int l = level; l >= 0; l-- )
            {
                if ( !_decodedLevels.TryGetValue( l, out DecodedMipLevel entry ) )
                {
                    GameLogger.LogError( $"Streaming: '{Handle}' is missing mip data for level {l} while applying level {level}." );
                    return;
                }
                totalBytes += entry.Bytes.Length;
            }

            var combined = new byte[ totalBytes ];
            int offset = 0;
            for ( int l = level; l >= 0; l-- )
            {
                byte[] bytes = _decodedLevels[ l ].Bytes;
                Buffer.BlockCopy( bytes, 0, combined, offset, bytes.Length );
                offset += bytes.Length;
            }

            DecodedMipLevel top = _decodedLevels[ level ];
            Image image = Image.CreateFromData( top.Width, top.Height, level > 0, top.Format, combined );

            _texture.SetImage( image );
            _onTextureReplaced( _texture );
        }
    }
}
