// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Streaming
{
    /// <summary>
    /// One resource's packed chunk table plus whichever chunks are currently loaded in memory.
    /// A chunk here is raw, still-encoded bytes (e.g. a BC7 mip, or a meshopt-encoded LOD) -
    /// decoding into whatever the engine actually consumes is the glue layer's job, done after
    /// <see cref="LoadChunkAsync"/> hands the bytes back.
    /// </summary>
    public sealed class ChunkStorage
    {
        private readonly IChunkDataSource _dataSource;
        private readonly string _url;
        private ChunkDescriptor[] _chunks = Array.Empty<ChunkDescriptor>();
        private byte[]?[] _loaded = Array.Empty<byte[]?>();
        private DateTime[] _lastAccess = Array.Empty<DateTime>();

        public string Url => _url;
        public int ChunkCount => _chunks.Length;
        public bool HeaderLoaded { get; private set; }

        internal ChunkStorage( IChunkDataSource dataSource, string url )
        {
            _dataSource = dataSource;
            _url = url;
        }

        /// <summary>Reads the chunk table itself. Cheap - always done before a resource can stream anything.</summary>
        public async Task LoadHeaderAsync( CancellationToken cancellationToken )
        {
            if ( HeaderLoaded )
                return;

            ChunkStorageHeader header = await _dataSource.ReadHeaderAsync( _url, cancellationToken )
                .ConfigureAwait( false );

            _chunks = header.Chunks;
            _loaded = new byte[]?[ _chunks.Length ];
            _lastAccess = new DateTime[ _chunks.Length ];
            HeaderLoaded = true;
        }

        public bool IsChunkLoaded( int index ) => _loaded[ index ] != null;

        /// <summary>Sum of chunk 0..level's declared sizes, straight from the header - used for memory-budget accounting, not actual live memory (which would also include decode overhead the glue layer adds).</summary>
        public long GetSizeUpTo( int level )
        {
            long total = 0;
            for ( int i = 0; i <= level && i < _chunks.Length; i++ )
                total += _chunks[ i ].Size;
            return total;
        }

        public ReadOnlyMemory<byte> GetLoadedChunk( int index )
        {
            _lastAccess[ index ] = DateTime.UtcNow;
            byte[]? data = _loaded[ index ];
            return data == null ? ReadOnlyMemory<byte>.Empty : data;
        }

        /// <summary>Loads one chunk's bytes if not already resident. Safe to call repeatedly; a second call while one is in flight awaits the same read.</summary>
        public async Task LoadChunkAsync( int index, CancellationToken cancellationToken )
        {
            if ( _loaded[ index ] != null )
                return;

            byte[] data = await _dataSource.ReadChunkAsync( _url, _chunks[ index ], cancellationToken )
                .ConfigureAwait( false );

            _loaded[ index ] = data;
            _lastAccess[ index ] = DateTime.UtcNow;
        }

        public void UnloadChunk( int index ) => _loaded[ index ] = null;

        /// <summary>
        /// Frees any chunk above <paramref name="residencyFloor"/> that hasn't been touched in
        /// <paramref name="lifetime"/>. Never touches index 0..<paramref name="residencyFloor"/>,
        /// even if it's gone untouched far longer than the lifetime - "chunk N is part of the
        /// current residency" is an invariant other code relies on (see StreamableResource's
        /// descending branch, which re-applies whatever's already loaded at the target level
        /// without re-checking it's actually there), not a cache with a soft expiry. An earlier
        /// version of this method evicted purely by last-access time regardless of residency,
        /// which could silently strand a mesh or texture on a stale level while Core believed the
        /// higher one had been reached - fixed by taking the floor as a required parameter.
        /// </summary>
        public void ReleaseStaleChunks( TimeSpan lifetime, int residencyFloor )
        {
            DateTime now = DateTime.UtcNow;
            int start = Math.Max( 0, residencyFloor + 1 );
            for ( int i = start; i < _loaded.Length; i++ )
            {
                if ( _loaded[ i ] != null && now - _lastAccess[ i ] >= lifetime )
                    _loaded[ i ] = null;
            }
        }

        /// <summary>
        /// Offline packing: writes a self-contained container to <paramref name="outputPath"/> -
        /// a small header table (chunk count, then each chunk's offset/size) immediately followed
        /// by the chunk bytes themselves, in order. This is the exact layout <see cref="LoadHeaderAsync"/>
        /// expects back via <see cref="IChunkDataSource"/> - Core owns the format end to end, the
        /// baker just calls this after it has already produced mip/LOD bytes with whatever
        /// external tool it uses.
        ///
        /// Each <see cref="ChunkDescriptor.Offset"/> is absolute (from the start of the file),
        /// not relative to where the chunk data begins - it's cheap to compute once here (the
        /// header size is known up front from the chunk count), and it means a reader never has
        /// to re-derive the header size itself before every single chunk read, the way an earlier
        /// version of this format required.
        /// </summary>
        public static ChunkStorageHeader Pack( string outputPath, IReadOnlyList<byte[]> chunkData )
        {
            if ( chunkData == null || chunkData.Count == 0 )
                throw new ArgumentException( "At least one chunk (the always-resident lowest level) is required.", nameof( chunkData ) );

            long headerSize = 4 + chunkData.Count * 12L; // int32 count + count * (int64 offset, int32 size)
            var descriptors = new ChunkDescriptor[ chunkData.Count ];
            long offset = headerSize;
            for ( int i = 0; i < chunkData.Count; i++ )
            {
                descriptors[ i ] = new ChunkDescriptor( offset, chunkData[ i ].Length );
                offset += chunkData[ i ].Length;
            }

            using ( FileStream stream = File.Create( outputPath ) )
            using ( var writer = new BinaryWriter( stream ) )
            {
                writer.Write( chunkData.Count );
                foreach ( ChunkDescriptor d in descriptors )
                {
                    writer.Write( d.Offset );
                    writer.Write( d.Size );
                }
                foreach ( byte[] bytes in chunkData )
                    writer.Write( bytes );
            }

            return new ChunkStorageHeader( descriptors );
        }
    }
}
