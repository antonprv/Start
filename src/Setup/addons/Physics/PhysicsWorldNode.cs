// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;

using Physics.Types;
using System.Collections.Generic;

namespace Physics.Autoload
{
	/// <summary>
	/// Add one instance of this to your scene tree (commonly as an autoload, e.g. "/root/PhysicsWorld").
	/// Owns the engine-agnostic <see cref="Framework.Physics.PhysicsWorld"/>, steps it every
	/// physics tick, and turns its plain <see cref="OverlapEvent"/> results (which only carry
	/// integer owner ids - Core has no idea what a Node3D is) into
	/// <see cref="IPhysicsCollisionListener"/> calls on the actual Godot nodes.
	///
	/// This is the only place in the whole physics stack where "a Bepu collidable" and "a Godot
	/// Node3D" are the same sentence. Everything below this (Framework.Physics) only ever
	/// deals with owner ids; everything above it (BepuBody3D and friends) only ever deals with
	/// Node3D and Core's plain result structs.
	/// </summary>
	public partial class PhysicsWorldNode : Node, IPhysicsWorld
	{
		[ExportGroup( "Simulation" )]
		[Export] private Vector3 _gravity = new Vector3( 0f, -20f, 0f );
		[Export] private int _velocityIterations = 8;
		[Export] private int _substeps = 1;
		[Export] private bool _useMultithreading = true;
		[Export] private float _frictionCoefficient = 0.8f;
		[Export] private float _maximumRecoveryVelocity = 2f;

		#region IPhysicsWorld CodeOnly

		public Vector3Packed Gravity => _gravityPacked;

		#endregion

		public Framework.Physics.PhysicsWorld Core { get; private set; } = null!;

		private Vector3Packed _gravityPacked;
		private int _nextOwnerId = 1;
		private readonly Dictionary<int, Node3D> _ownerNodes = new Dictionary<int, Node3D>();

		public override void _EnterTree() =>
			_gravityPacked = new Vector3Packed( _gravity );

		public override void _Ready()
		{
			PhysicsWorldSettings settings = new PhysicsWorldSettings(
				new System.Numerics.Vector3( _gravity.X, _gravity.Y, _gravity.Z ),
				_velocityIterations,
				_substeps,
				_useMultithreading,
				_frictionCoefficient,
				_maximumRecoveryVelocity );

			Core = new Framework.Physics.PhysicsWorld( settings );
			GD.Print( $"[framework_physics] PhysicsWorld initialized (threads: {Core.ThreadCount})." );
		}

		public override void _PhysicsProcess( double delta )
		{
			List<OverlapEvent> events = Core.Step( (float)delta );
			foreach ( OverlapEvent overlapEvent in events )
				Dispatch( overlapEvent );
		}

		public override void _ExitTree() => Core?.Dispose();

		public int RegisterOwner( Node3D node )
		{
			int id = _nextOwnerId++;
			_ownerNodes[ id ] = node;
			return id;
		}

		public void UnregisterOwner( int ownerId ) => _ownerNodes.Remove( ownerId );

		public Node3D? GetOwner( int ownerId ) => _ownerNodes.TryGetValue( ownerId, out Node3D? node ) ? node : null;

		private void Dispatch( OverlapEvent overlapEvent )
		{
			Node3D? nodeA = GetOwner( overlapEvent.OwnerIdA );
			Node3D? nodeB = GetOwner( overlapEvent.OwnerIdB );
			if ( nodeA == null || nodeB == null )
				return;

			if ( overlapEvent.Entered )
			{
				( nodeA as IPhysicsCollisionListener )?.OnPhysicsBodyEntered( nodeB );
				( nodeB as IPhysicsCollisionListener )?.OnPhysicsBodyEntered( nodeA );
			}
			else
			{
				( nodeA as IPhysicsCollisionListener )?.OnPhysicsBodyExited( nodeB );
				( nodeB as IPhysicsCollisionListener )?.OnPhysicsBodyExited( nodeA );
			}
		}
	}
}
