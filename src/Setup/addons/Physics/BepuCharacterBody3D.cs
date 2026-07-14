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
				(float)delta,
				_radius,
				_cylinderLength,
				(uint)Layer,
				(uint)Mask,
				options );

			IsOnFloor.Value = result.IsOnFloor;
			FloorNormal = GodotShapeConverter.ToGodot( result.FloorNormal );
			SVec3 actualDisplacement = result.Position - GodotShapeConverter.ToNumerics( GlobalPosition );
			GlobalPosition = GodotShapeConverter.ToGodot( result.Position );

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
