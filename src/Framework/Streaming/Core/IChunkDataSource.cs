// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Threading;
using System.Threading.Tasks;

namespace Framework.Streaming
{
    /// <summary>
    /// The one door Core uses to touch actual bytes on disk. Core never opens a file, never sees
    /// a "res://" path, never knows Godot's FileAccess exists - it only asks a data source it was
    /// handed to read N bytes at some offset from some opaque url. The glue layer supplies the
    /// real implementation (see GodotChunkDataSource); Core can be unit-tested against an
    /// in-memory fake without ever touching Godot.
    /// </summary>
    public interface IChunkDataSource
    {
        /// <summary>
        /// Reads <paramref name="chunk"/>'s raw bytes from <paramref name="url"/> into a fresh buffer.
        /// Expected to run off the caller's thread; implementations that can't do real async I/O
        /// should wrap themselves in <see cref="Task.Run(System.Action)"/> rather than block here.
        /// </summary>
        Task<byte[]> ReadChunkAsync( string url, ChunkDescriptor chunk, CancellationToken cancellationToken );

        /// <summary>Reads the small fixed-size header (chunk table) that precedes a resource's packed data.</summary>
        Task<ChunkStorageHeader> ReadHeaderAsync( string url, CancellationToken cancellationToken );
    }

    /// <summary>
    /// The chunk table for one packed resource file - written once by the offline baker, read
    /// once when a <see cref="StreamableResource"/> is first registered.
    /// </summary>
    public readonly struct ChunkStorageHeader
    {
        public readonly ChunkDescriptor[] Chunks;

        public ChunkStorageHeader( ChunkDescriptor[] chunks )
        {
            Chunks = chunks;
        }
    }
}
