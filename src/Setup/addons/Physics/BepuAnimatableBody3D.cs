// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Numerics;
using Framework.Logger;
using Framework.Physics;
using Godot;
using NumericsQuaternion = System.Numerics.Quaternion;
using NumericsVector3 = System.Numerics.Vector3;

namespace Physics
{
	/// <summary>
	/// A kinematic body driven entirely by external transform changes - Tweens,
	/// AnimationPlayer, code, whatever moves this node's transform. Mirrors Godot's stock
	/// AnimatableBody3D: it never reacts to forces or contacts itself, but every other
	/// physics object still collides against it and gets pushed along for the ride.
	///
	/// Shape comes from either a convex <see cref="CollisionShape3D"/> or a
	/// <see cref="MeshInstance3D"/>, same as <see cref="BepuStaticBody3D"/> - the only
	/// difference is this one is registered as a kinematic body instead of a static, so its
	/// pose can change every tick.
	///
	/// Each physics tick this teleports the underlying body's pose to match
	/// <see cref="Node3D.GlobalTransform"/> AND derives a linear/angular velocity from the
	/// pose delta. Both matter: the pose keeps this body visually glued to the animation,
	/// while the velocity is what the solver actually uses to compute contact response - a
	/// kinematic body that's only teleported (zero velocity) won't transfer any motion to a
	/// dynamic body resting on top of it, since Bepu's friction/push constraints are built
	/// from relative velocity, not from a pose diff no one told the solver about.
	/// </summary>
	[GlobalClass]
	public partial class BepuAnimatableBody3D : BepuBody3D
	{
		[ExportGroup( "Source" )]
		[Export] private CollisionShape3D? _convexShape;
		[Export] private MeshInstance3D? _meshShape;

		public BodyHandle Handle { get; private set; }
		private Vector3 _localOffset;
		private PhysicsTransform _lastPose;

		protected override void OnRegister()
		{
			ProcessPhysicsPriority = -100;

			BuiltShape built;

			GetReferences();

			if ( _convexShape != null )
			{
				built = GodotShapeConverter
					.FromCollisionShape3D( World, _convexShape, mass: 0f );
			}
			else if ( _meshShape != null )
			{
				built = GodotShapeConverter
					.FromMeshInstance3D( World, _meshShape, GlobalTransform.Basis.Scale );
			}
			else
			{
				GameLogger.LogError( $"{Name}: BepuAnimatableBody3D needs either " +
					$"a CollisionShape3D or MeshInstance3D assigned." );
				return;
			}

			_localOffset = built.LocalOffset;

			PhysicsTransform pose = GodotShapeConverter
				.ToPhysicsTransform( GlobalTransform, _localOffset );

			Handle = World.Core
				.AddKinematicBody( pose, built.Handle, (uint)Layer, (uint)Mask, PhysicsId );

			_lastPose = pose;
		}

		private void GetReferences()
		{
			if ( _convexShape == null && _meshShape == null )
			{
				foreach ( var item in GetChildren() )
				{
					if ( item is CollisionShape3D )
						_convexShape = (CollisionShape3D)item;

					if ( item is MeshInstance3D )
						_meshShape = (MeshInstance3D)item;
				}
			}
		}

		protected override void OnUnregister() => World.Core.RemoveBody( Handle );

		public override void _PhysicsProcess( double delta )
		{
			float dt = (float)delta;
			if ( dt <= 0f || !World.Core.BodyExists( Handle ) )
				return;

			PhysicsTransform targetPose = GodotShapeConverter
				.ToPhysicsTransform( GlobalTransform, _localOffset );

			NumericsVector3 linearVelocity = ( targetPose.Position - _lastPose.Position ) / dt;
			World.Core.SetLinearVelocity( Handle, linearVelocity );
			World.Core.SetAngularVelocity( Handle, AngularVelocityBetween( _lastPose.Orientation, targetPose.Orientation, dt ) );

			// Teleport the pose directly rather than letting the velocity-based integrator
			// approximate it over the tick - keeps this body pixel-perfect glued to whatever
			// is driving GlobalTransform (a Tween's easing curve isn't linear, so integrating
			// the velocity we just derived would drift from the actual animated pose).
			World.Core.SetBodyPose( Handle, targetPose );

			_lastPose = targetPose;
		}

		/// <summary>
		/// Angular velocity (axis * radians/sec) that would carry <paramref name="from"/> to
		/// <paramref name="to"/> over <paramref name="dt"/> seconds - the quaternion-log
		/// equivalent of the linear (delta position / dt) velocity above.
		/// </summary>
		private static NumericsVector3 AngularVelocityBetween( NumericsQuaternion from, NumericsQuaternion to, float dt )
		{
			NumericsQuaternion deltaRotation = FMath.Multiply( to, FMath.Inverse( from ) );
			NumericsQuaternion log = FMath.Log( deltaRotation );
			return new NumericsVector3( log.X, log.Y, log.Z ) * ( 2f / dt );
		}
	}
}