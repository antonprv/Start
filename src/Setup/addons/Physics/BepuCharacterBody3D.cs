// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Godot;
using Framework.Physics;
using Godot;
using Physics.Contracts;
using Physics.Types;
using SVec3 = System.Numerics.Vector3;
namespace Physics
{
	/// <summary>
	/// Kinematic capsule character controller. Drop-in replacement for Godot's CharacterBody3D
	/// for code that already computes a velocity and calls MoveAndSlide() once per physics tick -
	/// set <see cref="Velocity"/>, call <see cref="MoveAndSlide"/>, then check
	/// <see cref="IsOnFloor"/>, exactly like before.
	///
	/// All of the actual collide-and-slide sweep logic lives in
	/// <see cref="Framework.Physics.PhysicsWorld.MoveCharacter"/> - this component's job is
	/// purely to marshal Godot types in, marshal Core's plain result back out, and keep the
	/// registered kinematic body's pose in sync so other dynamic bodies still see and get pushed
	/// by the character normally.
	/// </summary>
	[GlobalClass]
	public partial class BepuCharacterBody3D : BepuBody3D
	{
		[ExportGroup( "Shape" )]
		[Export] private CapsuleBearer _capsuleBearer;

		[ExportGroup( "Sliding" )]
		[Export] private int _maxSlideIterations = 4;
		[Export] private float _skinWidth = 0.015f;
		[Export( PropertyHint.Range, "0,89,1" )] private float _maxFloorAngleDegrees = 46f;
		[Export] private float _floorProbeDistance = 0.08f;

		public BodyHandle Handle { get; private set; }
		public Vector3Packed Velocity { get; set; } = new Vector3Packed( Vector3.Zero );
		public BoolPacked IsOnFloor { get; private set; } = new BoolPacked( false );
		public Vector3 FloorNormal { get; private set; } = Vector3.Up;

		public Vector3Packed Gravity { get; private set; }

		// Which owner (if any) we were standing on as of the *previous* MoveAndSlide call.
		// MoveCharacter itself is a pure sweep query against wherever things are right now -
		// it has no concept of "the floor moved since last tick, bring me with it" - so that
		// carry has to happen explicitly, one tick behind, before the next sweep.
		private int _groundOwnerId;

		private CapsuleShape3D _capsule;
		private float _radius;
		private float _height;
		private float _halfHeight;
		private float _cylinderLength;

		protected override void OnRegister()
		{
			InitializeFields();

			_cylinderLength = FMath.Max( 0.01f, _height - 2f * _radius );

			ShapeHandle shapeHandle = World.Core
				.AddCapsuleShape(
				_capsule.Radius,
				_cylinderLength
			);

			PhysicsTransform pose = PhysicsTransform
				.FromPosition( GodotShapeConverter.ToNumerics( GlobalPosition ) );

			Handle = World.Core.AddKinematicBody(
				pose,
				shapeHandle,
				(uint)Layer,
				(uint)Mask,
				PhysicsId,
				PhysicsObjectKind.Character
			);
		}

		private void InitializeFields()
		{
			_capsule = _capsuleBearer.Capsule;

			_radius = _capsule.Radius;
			_height = _capsule.Height;
			_halfHeight = _capsule.MidHeight;

			Gravity = World.Gravity;
		}

		protected override void OnUnregister() => World.Core.RemoveBody( Handle );

		/// <summary>
		/// Moves the character by <see cref="Velocity"/> * delta, sliding along anything it hits.
		/// Mirrors Godot's CharacterBody3D.MoveAndSlide() contract: set Velocity, call this once
		/// per physics tick, then read IsOnFloor.
		/// </summary>
		public void MoveAndSlide( double delta )
		{
			float dt = (float)delta;

			// Carry: MoveCharacter is a pure sweep query against wherever things are *right
			// now* - it has no notion of "the floor moved since last tick, bring me with it."
			// So if we were standing on something last tick, ride along with its motion since
			// then before sweeping this tick's own input velocity. One tick behind is fine
			// (same as Godot's own get_platform_velocity() timing) - what matters is this runs
			// every tick the character is grounded, not just the ones where input changes.
			if ( IsOnFloor.Value && _groundOwnerId != 0 )
			{
				SVec3 platformVelocity = GetPlatformVelocity( World.GetOwner( _groundOwnerId ) );
				GlobalPosition += GodotShapeConverter.ToGodot( platformVelocity ) * dt;
			}

			CharacterMoveOptions options = new CharacterMoveOptions(
				_maxSlideIterations,
				_skinWidth,
				_maxFloorAngleDegrees,
				_floorProbeDistance
			);

			CharacterMoveResult result = World.Core.MoveCharacter(
				Handle,
				GodotShapeConverter.ToNumerics( GlobalPosition ),
				GodotShapeConverter.ToNumerics( Velocity.Value ),
				dt,
				_radius,
				_cylinderLength,
				(uint)Layer,
				(uint)Mask,
				options );

			IsOnFloor.Value = result.IsOnFloor;
			FloorNormal = GodotShapeConverter.ToGodot( result.FloorNormal );
			_groundOwnerId = result.GroundOwnerId;
			GlobalPosition = GodotShapeConverter.ToGodot( result.Position );

			// Feed the plane-clipped velocity back, same as id's current.velocity = clipVelocity
			// and same as Godot's own CharacterBody3D.MoveAndSlide(). Without this, a wall/corner
			// hit stops the position but leaves Velocity pointing full-speed into the obstacle,
			// so the next tick's acceleration has to fight that stale vector down instead of
			// starting clean - the corner-stuck bug.
			Velocity.Value = GodotShapeConverter.ToGodot( result.Velocity );

			World.Core.SetBodyPose(
				Handle,
				PhysicsTransform
					.FromPosition( GodotShapeConverter.ToNumerics( GlobalPosition ) )
			);

			World.Core.SetLinearVelocity(
				Handle,
				delta > 0 ? GodotShapeConverter.ToNumerics( Velocity.Value ) : SVec3.Zero
			);

		}

		/// <summary>
		/// Linear velocity of whatever <paramref name="groundNode"/> is, if it's a body type
		/// that can actually move (kinematic platforms, pushable rigid props). Statics never
		/// move so they're not checked; anything else (null, a trigger, etc.) contributes zero.
		/// </summary>
		private SVec3 GetPlatformVelocity( BepuBody3D? groundNode ) => groundNode switch
		{
			BepuAnimatableBody3D platform => World.Core.GetLinearVelocity( platform.Handle ),
			BepuRigidBody3D rigid => World.Core.GetLinearVelocity( rigid.Handle ),
			_ => SVec3.Zero
		};

		/// <summary>Teleport without sliding (e.g. respawn, cutscenes). Keeps the Core body in sync.</summary>
		public void Teleport( Vector3 worldPosition )
		{
			GlobalPosition = worldPosition;

			World.Core.SetBodyPose(
				Handle,
				PhysicsTransform
					.FromPosition( GodotShapeConverter.ToNumerics( worldPosition ) )
			);
		}
	}
}