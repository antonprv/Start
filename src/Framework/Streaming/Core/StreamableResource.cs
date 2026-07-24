// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Streaming
{
    /// <summary>
    /// Base class for anything that can be dynamically streamed - a texture's mip chain, a mesh's
    /// LOD chain. Owns the chunk table and residency bookkeeping; a glue-layer subclass (see
    /// StreamableTexture2D / StreamableMeshInstance3D) fills in what "prepare/apply chunk N"
    /// actually means for that engine's texture/mesh objects.
    ///
    /// Core drives this purely through integer residency levels - it has no idea a level is a
    /// mip or an LOD, only that higher is more detailed and every chunk below the target must be
    /// loaded before that level counts as reached.
    /// </summary>
    public abstract class StreamableResource
    {
        private readonly ChunkStorage _storage;
        private readonly ConcurrentQueue<(int Level, object? Prepared)> _pendingApply = new();
        private CancellationTokenSource? _streamingCts;
        private Task? _streamingTask;

        protected StreamableResource( StreamingWorld world, string url, StreamableKind kind, Vector3 worldPosition )
        {
            World = world;
            Kind = kind;
            WorldPosition = worldPosition;
            _storage = world.OpenStorage( url );
            Handle = world.RegisterResource( this, out _ );
        }

        public StreamingWorld World { get; }
        public StreamableHandle Handle { get; }
        public StreamableKind Kind { get; }
        public Vector3 WorldPosition { get; set; }

        /// <summary>True once the chunk table has been read and this resource is eligible for scheduling.</summary>
        public bool HeaderReady { get; private set; }
        internal void MarkHeaderReady() => HeaderReady = true;

        /// <summary>Highest level this resource can ever reach (chunk count - 1).</summary>
        public int MaxResidency => _storage.ChunkCount - 1;

        /// <summary>Highest level whose chunk is currently loaded and applied.</summary>
        public int CurrentResidency { get; private set; } = -1;

        /// <summary>Level the scheduler wants this resource at, given the last known viewer distance.</summary>
        public int TargetResidency { get; internal set; }

        /// <summary>Declared byte size of every chunk from 0 up to <see cref="CurrentResidency"/> - used by <see cref="StreamingWorld"/> for memory-budget accounting.</summary>
        public long ResidentBytes => CurrentResidency < 0 ? 0 : _storage.GetSizeUpTo( CurrentResidency );

        /// <summary>Declared byte size of every chunk from 0 up to an arbitrary (not necessarily current) level - used to project cost before committing to a target.</summary>
        internal long EstimateBytesAtLevel( int level ) => level < 0 ? 0 : _storage.GetSizeUpTo( level );

        internal bool IsStreamingTaskActive => _streamingTask != null && !_streamingTask.IsCompleted;
        internal bool CanBeScheduled => _streamingCts == null || _streamingTask!.IsCompleted;

        /// <summary>
        /// Decodes one chunk's raw bytes into whatever intermediate form the engine step needs
        /// (e.g. a decoded Image, an unpacked vertex buffer). Runs on a background thread as part
        /// of the streaming task - must not touch any engine object here, since most engine APIs
        /// (Godot's included) are only safe to call from the main thread.
        /// </summary>
        protected abstract object? PrepareChunk( int level, ReadOnlyMemory<byte> chunkData );

        /// <summary>
        /// Applies one chunk's already-decoded payload (as produced by <see cref="PrepareChunk"/>)
        /// to the actual engine resource - upload a mip, rebuild an ArrayMesh surface, etc. Only
        /// ever called from <see cref="StreamingWorld.Update"/> on the thread that owns the engine,
        /// never directly from the background streaming task.
        /// </summary>
        protected abstract void ApplyPreparedChunk( int level, object? prepared );

        /// <summary>Called once, the first time this resource streams in at all, before any ApplyChunk. Good place to create the placeholder engine object.</summary>
        protected virtual void OnFirstResidency() { }

        internal async Task LoadHeaderAsync( CancellationToken cancellationToken ) =>
            await _storage.LoadHeaderAsync( cancellationToken ).ConfigureAwait( false );

        /// <summary>Kicks off a background load of every chunk between the current and target residency, then applies them on the calling (main) thread.</summary>
        internal Task StreamToTargetAsync()
        {
            _streamingCts?.Dispose();
            _streamingCts = new CancellationTokenSource();
            _streamingTask = RunAsync( _streamingCts.Token );
            return _streamingTask;
        }

        private async Task RunAsync( CancellationToken cancellationToken )
        {
            bool firstResidency = CurrentResidency < 0;
            int from = Math.Max( CurrentResidency + 1, 0 );
            int to = TargetResidency;
            bool ascending = to >= from;
            int step = ascending ? 1 : -1;

            if ( firstResidency )
                OnFirstResidency();

            // Descending (viewer moved away) never needs disk I/O - just drop chunks above the new
            // target and re-apply the target level's (already loaded) chunk so the engine resource
            // actually reflects the lower detail, not just frees memory nobody points at anymore.
            if ( !ascending )
            {
                for ( int level = CurrentResidency; level > to; level-- )
                    _storage.UnloadChunk( level );

                object? prepared = to >= 0 ? PrepareChunk( to, _storage.GetLoadedChunk( to ) ) : null;
                _pendingApply.Enqueue( (to, prepared) );
                return;
            }

            for ( int level = from; level <= to; level += step )
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _storage.LoadChunkAsync( level, cancellationToken ).ConfigureAwait( false );
                object? prepared = PrepareChunk( level, _storage.GetLoadedChunk( level ) );
                _pendingApply.Enqueue( (level, prepared) );
                // Note: CurrentResidency only advances once FlushPendingApplications actually
                // applies the chunk on the main thread - see below.
            }
        }

        /// <summary>
        /// Drains whatever chunks finished background preparation since the last call and applies
        /// them in order. Called by <see cref="StreamingWorld.Update"/> - always on the thread
        /// that owns the engine, so subclasses are free to touch Godot objects from ApplyPreparedChunk.
        /// </summary>
        internal void FlushPendingApplications()
        {
            while ( _pendingApply.TryDequeue( out (int Level, object? Prepared) entry ) )
            {
                ApplyPreparedChunk( entry.Level, entry.Prepared );
                CurrentResidency = entry.Level;
            }
        }

        internal void ReleaseStaleChunks( TimeSpan lifetime ) => _storage.ReleaseStaleChunks( lifetime, CurrentResidency );

        public void Unregister() => World.UnregisterResource( this );
    }
}
