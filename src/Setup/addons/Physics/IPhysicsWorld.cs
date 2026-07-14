// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using Physics.Types;

namespace Physics
{
	/// <summary>
	/// Implemented by any Node3D-based physics component that wants Entered/Exited
	/// notifications for overlaps (solid touches AND triggers).
	/// </summary>
	public interface IPhysicsCollisionListener
	{
		void OnPhysicsBodyEntered( BepuBody3D other );
		void OnPhysicsBodyExited( BepuBody3D other );
	}

	/// <summary>Implemented by projectile owners that want a single unambiguous "first hit" callback.</summary>
	public interface IProjectileHitListener
	{
		void OnProjectileHit( Vector3 point, Vector3 normal, BepuBody3D? hitOwner );
	}

	/// <summary>
	/// The Godot-facing physics service. Wraps a <see cref="Framework.Physics.PhysicsWorld"/>
	/// (exposed directly via <see cref="Core"/> for components to call) plus the one thing Core
	/// deliberately knows nothing about: which Godot <see cref="Node3D"/> owns which collidable.
	/// </summary>
	public interface IPhysicsWorld
	{
		/// <summary>The engine-agnostic simulation facade. 
		/// Its API only uses plain data types (see Framework.Physics) - never a Godot type,
		/// never a raw Bepu type.</summary>
		Framework.Physics.PhysicsWorld Core { get; }
		Vector3Packed Gravity { get; }

		int RegisterOwner( BepuBody3D node );
		void UnregisterOwner( int ownerId );
		BepuBody3D? GetOwner( int ownerId );
	}
}
