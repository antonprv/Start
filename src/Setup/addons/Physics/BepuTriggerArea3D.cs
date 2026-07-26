// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Logger;
using Framework.Physics;
using Godot;
using System.Collections.Generic;


namespace Physics
{
	/// <summary>
	/// Non-solid sensor volume - the BEPU equivalent of Godot's Area3D. Registered with
	/// <see cref="PhysicsObjectKind.Trigger"/> so Core's narrow phase still generates contact
	/// manifolds for overlap detection but never emits a solver constraint - nothing physically
	/// collides with it.
	///
	/// Emits <c>BodyEntered</c>/<c>BodyExited</c> Godot signals for scenes wired up in the editor,
	/// and implements <see cref="IPhysicsCollisionListener"/> so sibling C# components can just
	/// override those methods directly.
	///
	/// If you need a trigger that MOVES (e.g. attached to a moving platform or the player), flip
	/// <see cref="_buildAsStatic"/> off - this registers a kinematic body instead, whose pose is
	/// synced to wherever this node has moved to each frame.
	/// </summary>
	[GlobalClass]
	public partial class BepuTriggerArea3D : BepuBody3D, IPhysicsCollisionListener
	{
		[ExportGroup( "Source" )]
		[Export] private CollisionShape3D _shapeSource = null!;

		[ExportGroup( "Behavior" )]
		[Export] private bool _buildAsStatic = true;

		private readonly HashSet<int> _bodiesInside = new HashSet<int>();

		[Signal] public delegate void BodyEnteredEventHandler( Node3D body );
		[Signal] public delegate void BodyExitedEventHandler( Node3D body );

		public StaticHandle StaticHandleValue { get; private set; }
		public BodyHandle BodyHandleValue { get; private set; }

		public CollisionShape3D ShapeSource { get; private set; }

		protected override void OnRegister()
		{
			if ( _shapeSource == null )
			{
				GameLogger.LogError( $"{Name}: BepuTriggerArea3D requires " +
					$"a CollisionShape3D assigned to ShapeSource." );
				return;
			}

			ShapeSource = _shapeSource;

			BuiltShape built = GodotShapeConverter.FromCollisionShape3D( World, _shapeSource, mass: 0f );
			PhysicsTransform pose = GodotShapeConverter.ToPhysicsTransform( GlobalTransform, built.LocalOffset );

			if ( _buildAsStatic )
			{
				StaticHandleValue = World.Core.AddStatic(
					pose: pose,
					shape: built.Handle,
					layer: (uint)Layer,
					mask: (uint)Mask,
					ownerId: PhysicsId,
					kind: PhysicsObjectKind.Trigger
				);
			}
			else
			{
				BodyHandleValue = World.Core.AddKinematicBody(
					pose: pose,
					shape: built.Handle,
					layer: (uint)Layer,
					mask: (uint)Mask,
					physicsId: PhysicsId,
					kind: PhysicsObjectKind.Trigger
				);
			}
		}

		protected override void OnUnregister()
		{
			if ( _buildAsStatic )
				World.Core.RemoveStatic( StaticHandleValue );
			else
				World.Core.RemoveBody( BodyHandleValue );
		}

		public override void _PhysicsProcess( double delta )
		{
			if ( _buildAsStatic )
				return;

			World.Core.SetBodyPose(
				BodyHandleValue,
				GodotShapeConverter.ToPhysicsTransform( GlobalTransform )
			);
		}

		public virtual void OnPhysicsBodyEntered( BepuBody3D other )
		{
			int id = other.PhysicsId;
			if ( _bodiesInside.Contains( id ) )
				return;

			_bodiesInside.Add( id );
			EmitSignal( SignalName.BodyEntered, other );
		}

		public virtual void OnPhysicsBodyExited( BepuBody3D other )
		{
			int id = other.PhysicsId;
			if ( !_bodiesInside.Contains( id ) )
				return;

			_bodiesInside.Remove( id );
			EmitSignal( SignalName.BodyExited, other );
		}
	}
}
