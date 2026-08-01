// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Streaming
{
	/// <summary>
	/// The engine-agnostic streaming facade - the direct analogue of Framework.Physics.PhysicsWorld.
	/// Owns every registered StreamableResource, decides (from a StreamingBudget and the last
	/// known viewer position) which ones should change residency this tick, and kicks off their
	/// background loads. Its public API only uses plain data types - never a Godot type, never a
	/// raw Bepu/RD type.
	/// </summary>
	public sealed class StreamingWorld : IDisposable
	{
		private readonly IChunkDataSource _dataSource;
		private readonly Dictionary<int, StreamableResource> _resources = new();
		private readonly List<StreamableResource> _pendingHeaderLoad = new();
		private int _nextHandle = 1;
		private StreamingViewer _viewer;

		public StreamingWorld( IChunkDataSource dataSource, StreamingBudget budget )
		{
			_dataSource = dataSource;
			Budget = budget;
		}

		public StreamingBudget Budget { get; set; }
		public IChunkDataSource DataSource => _dataSource;
		public int ResourceCount => _resources.Count;

		/// <summary>Feeds in this frame's viewer position/FOV - call once per tick before <see cref="Update"/>.</summary>
		public void SetViewer( StreamingViewer viewer ) => _viewer = viewer;

		internal ChunkStorage OpenStorage( string url ) => new ChunkStorage( _dataSource, url );

		/// <summary>
		/// Registers a resource that's already been constructed (its subclass constructor calls
		/// this via the base). Header load is deferred to the first <see cref="Update"/> so
		/// registering many resources on scene load doesn't stall waiting for disk one at a time.
		/// </summary>
		internal StreamableHandle RegisterResource( StreamableResource resource, out int handleValue )
		{
			handleValue = _nextHandle++;
			_resources[ handleValue ] = resource;
			_pendingHeaderLoad.Add( resource );
			return new StreamableHandle( handleValue );
		}

		public void UnregisterResource( StreamableResource resource )
		{
			foreach ( KeyValuePair<int, StreamableResource> pair in _resources )
			{
				if ( ReferenceEquals( pair.Value, resource ) )
				{
					_resources.Remove( pair.Key );
					return;
				}
			}
		}

		/// <summary>
		/// Advances streaming by one tick: computes each resource's target residency from
		/// distance to the last-set viewer, then starts background loads for as many resources
		/// as the byte/count budget allows, ordered nearest-first. Call once per frame (or on a
		/// slower fixed cadence - streaming rarely needs to run at render framerate).
		/// </summary>
		public void Update()
		{
			FlushPendingHeaderLoads();

			if ( _resources.Count == 0 )
				return;

			long remainingBytes = Budget.BytesPerTick;
			int remainingUpdates = Budget.MaxResourceUpdatesPerTick;

			// Cheapest possible priority queue: sort the (usually small) working set by distance
			// each tick rather than maintaining a persistent heap - fine up to a few hundred
			// streamable resources, which comfortably covers one FPS level's worth of props.
			foreach ( StreamableResource resource in _resources.Values )
			{
				// Always flush, even for resources that won't be rescheduled this tick - a
				// background load kicked off last tick may have finished in the meantime and is
				// waiting for exactly this main-thread-only step to actually reach the engine.
				resource.FlushPendingApplications();

				if ( !resource.HeaderReady )
					continue;

				resource.TargetResidency = CalculateTargetResidency( resource );
				resource.ReleaseStaleChunks( Budget.UnusedChunkLifetime );
			}

			// Memory budget can only ever pull targets down further, never push them up - it runs
			// after the distance-based pass so it always has the true desired targets to weigh
			// against each other before deciding who gives up detail first.
			if ( Budget.MemoryBudgetBytes != long.MaxValue )
				EnforceMemoryBudget();

			var candidates = new List<StreamableResource>( _resources.Count );
			foreach ( StreamableResource resource in _resources.Values )
			{
				if ( resource.HeaderReady && resource.TargetResidency != resource.CurrentResidency && resource.CanBeScheduled )
					candidates.Add( resource );
			}

			candidates.Sort( ( a, b ) => DistanceSqr( a.WorldPosition ).CompareTo( DistanceSqr( b.WorldPosition ) ) );

			foreach ( StreamableResource resource in candidates )
			{
				if ( remainingUpdates <= 0 || remainingBytes <= 0 )
					break;

				long estimatedCost = EstimateStepCost( resource );
				if ( estimatedCost > remainingBytes && resource.TargetResidency > resource.CurrentResidency )
					continue; // let a cheaper resource go first this tick; this one waits, it doesn't starve

				_ = resource.StreamToTargetAsync();
				remainingBytes -= estimatedCost;
				remainingUpdates--;
			}
		}

		private float DistanceSqr( Vector3 position ) => Vector3.DistanceSquared( position, _viewer.Position );

		private int CalculateTargetResidency( StreamableResource resource )
		{
			if ( resource.MaxResidency <= 0 )
				return 0;

			float distance = FMath.FastSqrt( DistanceSqr( resource.WorldPosition ) );
			float t = ( distance - Budget.FullDetailDistance ) /
				FMath.Max( 1f, Budget.MinDetailDistance - Budget.FullDetailDistance );

			t = FMath.Clamp( 1f - t, 0f, 1f );

			return (int)FMath.Round( t * resource.MaxResidency );
		}

		private static long EstimateStepCost( StreamableResource resource )
		{
			long from = resource.EstimateBytesAtLevel( resource.CurrentResidency );
			long to = resource.EstimateBytesAtLevel( resource.TargetResidency );
			return FMath.AbsBranchless( to - from );
		}

		/// <summary>
		/// If every resource reached its distance-computed target this would use more than
		/// <see cref="StreamingBudget.MemoryBudgetBytes"/>, claws detail back from the farthest
		/// resources first (least value per byte) until the projection fits, one level at a time
		/// so no single resource gets zeroed out just because it happens to be sorted first.
		/// </summary>
		private void EnforceMemoryBudget()
		{
			var tracked = new List<StreamableResource>( _resources.Count );
			long projected = 0;
			foreach ( StreamableResource resource in _resources.Values )
			{
				if ( !resource.HeaderReady )
					continue;
				tracked.Add( resource );
				projected += resource.EstimateBytesAtLevel( resource.TargetResidency );
			}

			if ( projected <= Budget.MemoryBudgetBytes )
				return;

			tracked.Sort( ( a, b ) => DistanceSqr( b.WorldPosition ).CompareTo( DistanceSqr( a.WorldPosition ) ) );

			bool reducedAny = true;
			while ( projected > Budget.MemoryBudgetBytes && reducedAny )
			{
				reducedAny = false;
				foreach ( StreamableResource resource in tracked )
				{
					if ( resource.TargetResidency <= 0 )
						continue;

					long before = resource.EstimateBytesAtLevel( resource.TargetResidency );
					resource.TargetResidency--;
					long after = resource.EstimateBytesAtLevel( resource.TargetResidency );
					projected -= before - after;
					reducedAny = true;

					if ( projected <= Budget.MemoryBudgetBytes )
						break;
				}
			}
			// If every resource bottomed out at level 0 and we're still over budget, that's not
			// something Core can fix by streaming less - the working set itself exceeds the
			// budget even at minimum detail. Left as-is deliberately rather than throwing: a game
			// pausing to error out over a VRAM budget miss would be worse than briefly exceeding it.
		}

		private void FlushPendingHeaderLoads()
		{
			if ( _pendingHeaderLoad.Count == 0 )
				return;

			foreach ( StreamableResource resource in _pendingHeaderLoad )
				_ = LoadHeaderThenMark( resource );

			_pendingHeaderLoad.Clear();
		}

		private static async Task LoadHeaderThenMark( StreamableResource resource )
		{
			await resource.LoadHeaderAsync( CancellationToken.None ).ConfigureAwait( false );
			resource.MarkHeaderReady();
		}

		public void Dispose()
		{
			_resources.Clear();
			_pendingHeaderLoad.Clear();
		}
	}
}
