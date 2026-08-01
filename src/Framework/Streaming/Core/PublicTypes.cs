// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Numerics;

namespace Framework.Streaming
{
	/// <summary>
	/// What kind of streamable data a resource holds (see StreamingWorld's scheduling rules).
	/// Core has no opinion on how a Texture or Mesh residency level is actually consumed by the
	/// engine - that (a GPU upload, an ArrayMesh rebuild, ...) is entirely a glue-layer concern.
	/// </summary>
	public enum StreamableKind : byte
	{
		/// <summary>Residency levels are mip levels, ordered from smallest (0) to full res.</summary>
		Texture = 0,
		/// <summary>Residency levels are LOD levels, ordered from coarsest (0) to most detailed.</summary>
		Mesh = 1,
	}

	/// <summary>Opaque reference to a resource registered with a <see cref="StreamingWorld"/>.</summary>
	public readonly struct StreamableHandle : IEquatable<StreamableHandle>
	{
		internal readonly int Value;
		internal StreamableHandle( int value ) => Value = value;
		public bool Equals( StreamableHandle other ) => Value == other.Value;
		public override bool Equals( object? obj ) => obj is StreamableHandle other && Equals( other );
		public override int GetHashCode() => Value;
		public static readonly StreamableHandle Invalid = new StreamableHandle( 0 );
	}

	/// <summary>
	/// Describes one chunk of a resource's on-disk data - one mip level's worth of pixels, or
	/// one LOD level's worth of vertex/index bytes. Chunks are the unit Core streams in and out;
	/// residency level N being loaded always implies every chunk with index &lt;= N is loaded too.
	/// </summary>
	public readonly struct ChunkDescriptor
	{
		/// <summary>Absolute byte offset of this chunk from the start of the resource's packed data file (not relative to where chunk data begins - see ChunkStorage.Pack).</summary>
		public readonly long Offset;
		/// <summary>Size in bytes of this chunk's packed (possibly still-compressed) data.</summary>
		public readonly int Size;

		public ChunkDescriptor( long offset, int size )
		{
			Offset = offset;
			Size = size;
		}
	}

	/// <summary>Where the viewer is, for distance-based residency decisions. One per frame is enough.</summary>
	public readonly struct StreamingViewer
	{
		public readonly Vector3 Position;
		/// <summary>Vertical field of view in radians. Wider FOV needs relatively more detail up close.</summary>
		public readonly float FieldOfView;

		public StreamingViewer( Vector3 position, float fieldOfView )
		{
			Position = position;
			FieldOfView = fieldOfView;
		}
	}

	/// <summary>
	/// Global tuning knobs for a <see cref="StreamingWorld"/> - how hard it's allowed to push disk
	/// I/O per tick and how it converts distance into a desired residency level. Deliberately plain
	/// data so a game can swap presets (e.g. "loading screen": no budget cap) at runtime.
	/// </summary>
	public readonly struct StreamingBudget
	{
		/// <summary>Max bytes of chunk data allowed to start loading in a single <see cref="StreamingWorld.Update"/> call.</summary>
		public readonly long BytesPerTick;
		/// <summary>Max number of resources allowed to change residency in a single tick, regardless of byte budget.</summary>
		public readonly int MaxResourceUpdatesPerTick;
		/// <summary>Distance at which a resource is expected to be at full residency.</summary>
		public readonly float FullDetailDistance;
		/// <summary>Distance beyond which a resource is dropped to its lowest residency level.</summary>
		public readonly float MinDetailDistance;
		/// <summary>How long an already-loaded chunk is kept around after it stops being requested,
		/// before Core reclaims it.</summary>
		public readonly TimeSpan UnusedChunkLifetime;
		/// <summary>Total resident bytes allowed across every registered resource 
		/// before Core starts forcing distant resources down a level early,
		/// regardless of their distance-computed target. long.MaxValue effectively disables the cap.</summary>
		public readonly long MemoryBudgetBytes;

		public StreamingBudget(
			long bytesPerTick = 2 * 1024 * 1024,
			int maxResourceUpdatesPerTick = 8,
			float fullDetailDistance = 8f,
			float minDetailDistance = 60f,
			TimeSpan? unusedChunkLifetime = null,
			long memoryBudgetBytes = long.MaxValue )
		{
			BytesPerTick = bytesPerTick;
			MaxResourceUpdatesPerTick = maxResourceUpdatesPerTick;
			FullDetailDistance = fullDetailDistance;
			MinDetailDistance = minDetailDistance;
			UnusedChunkLifetime = unusedChunkLifetime ?? TimeSpan.FromSeconds( 8 );
			MemoryBudgetBytes = memoryBudgetBytes;
		}

		public static StreamingBudget Default => new StreamingBudget();
	}
}
