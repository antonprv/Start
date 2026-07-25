// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;
using System.Threading.Tasks;
using Framework.Streaming;
using Godot;

namespace Setup.addons.Streaming
{
    /// <summary>
    /// Reads packed chunk containers baked by the offline packer (see ChunkStorage.Pack) via
    /// Godot's FileAccess. Godot's FileAccess has no real async API, so every read is pushed onto
    /// the thread pool with Task.Run - this is the only place in the streaming stack that blocks a
    /// thread on disk I/O, and it's never the main/render thread.
    ///
    /// Container layout on disk, written by the baker:
    ///   [int32 chunkCount][chunkCount * (int64 absoluteOffset, int32 size)][raw chunk bytes...]
    /// </summary>
    public sealed class GodotChunkDataSource : IChunkDataSource
    {
        public Task<ChunkStorageHeader> ReadHeaderAsync( string url, CancellationToken cancellationToken )
        {
            return Task.Run( () =>
            {
                using FileAccess file = FileAccess.Open( url, FileAccess.ModeFlags.Read );
                if ( file == null )
                    throw new InvalidOperationException( $"Streaming: failed to open '{url}' ({FileAccess.GetOpenError()})." );

                int chunkCount = (int)file.Get32();
                var chunks = new ChunkDescriptor[ chunkCount ];
                for ( int i = 0; i < chunkCount; i++ )
                {
                    long offset = (long)file.Get64();
                    int size = (int)file.Get32();
                    chunks[ i ] = new ChunkDescriptor( offset, size );
                }

                return new ChunkStorageHeader( chunks );
            }, cancellationToken );
        }

        public Task<byte[]> ReadChunkAsync( string url, ChunkDescriptor chunk, CancellationToken cancellationToken )
        {
            return Task.Run( () =>
            {
                using FileAccess file = FileAccess.Open( url, FileAccess.ModeFlags.Read );
                if ( file == null )
                    throw new InvalidOperationException( $"Streaming: failed to open '{url}' ({FileAccess.GetOpenError()})." );

                file.Seek( (ulong)chunk.Offset );
                return file.GetBuffer( chunk.Size );
            }, cancellationToken );
        }
    }
}
