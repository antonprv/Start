// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.FastMath.Godot;
using Framework.Physics;
using Godot;
using Zenjex;

namespace Physics
{
	/// <summary>
	/// Drop-in equivalent of Godot's native <c>SpringArm3D</c>, but the collision query goes
	/// through <see cref="Framework.Physics.PhysicsWorld.SweepSphereCast"/> instead of
	/// PhysicsServer3D - so it sees the same Bepu world every other component in this framework
	/// does, including static level geometry baked by func_godot.
	///
	/// Each physics tick it sweeps a sphere from this node's position along local +Z by up to
	/// <see cref="SpringLength"/>, then repositions every direct Node3D child to
	/// <c>(0, 0, CurrentLength)</c> - exactly matching native SpringArm3D's own convention (see
	/// spring_arm_3d.cpp: <c>cast_direction = basis.xform(Vector3(0, 0, 1))</c>, i.e. positive Z,
	/// not negative). This matters: +Z is "backward" in Godot's forward=-Z convention, so the arm
	/// extends behind the pivot and a child Camera3D with no extra rotation (default forward -Z)
	/// ends up looking back toward the pivot/character - the standard third-person arrangement.
	/// An earlier version of this file used -Z, which points the arm the opposite way - toward
	/// wherever the rig is already facing - so the camera ended up positioned past the character
	/// looking further away from it instead of positioned behind it looking back. That was the
	/// actual cause of the "camera is somewhere unclear" symptom.
	///
	/// Unlike the native node (which takes an arbitrary <c>Shape3D</c>), this only supports a
	/// sphere - that covers the overwhelming majority of camera-boom use cases and keeps the
	/// query a single cheap <c>Simulation.Sweep</c> call.
	/// </summary>
	[GlobalClass]
	public partial class BepuSpringArm3D : Node3D
	{
		[ExportGroup( "Collision" )]
		[Export] public CollisionLayer Layer { get; set; } = CollisionLayer.Character;
		[Export] public CollisionLayer Mask { get; set; } = CollisionLayer.World;
		[Export] public float Radius { get; set; } = 0.2f;
		[Export] public float Margin { get; set; } = 0.05f;

		[ExportGroup( "Spring" )]
		[Export] public float SpringLength { get; set; } = 4f;

		/// <summary>
		/// Optional - whatever this arm is mounted on (typically the player). Excluded from the
		/// sweep so the arm doesn't immediately collide with its own owner's capsule the instant
		/// it pokes out past it.
		/// </summary>
		[ExportGroup( "Owner" )]
		[Export] public BepuBody3D? ExcludeBody { get; set; }

		/// <summary>Current resolved arm length this frame - the equivalent of native SpringArm3D's GetHitLength().</summary>
		public float CurrentLength { get; private set; }

		public bool IsColliding { get; private set; }

		[Inject]
		private IPhysicsWorld _world = null!;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void _PhysicsProcess( double delta )
		{
			UpdateArm();
		}

		private void UpdateArm()
		{
			Vector3 origin = GlobalPosition;
			Vector3 direction = GlobalTransform.Basis.Z;
			BodyHandle? exclude = ResolveExcludeHandle();

			ShapeCastResult result = _world.Core.SweepSphereCast(
				origin: GodotShapeConverter.ToNumerics( origin ),
				direction: GodotShapeConverter.ToNumerics( direction ),
				maxDistance: SpringLength,
				radius: Radius,
				layer: (uint)Layer,
				mask: (uint)Mask,
				exclude: exclude
			);

			IsColliding = result.Hit;
			CurrentLength = result.Hit ? FMath.Max( 0f, result.Distance - Margin ) : SpringLength;

			Vector3 localTip = new Vector3( 0f, 0f, CurrentLength );

			foreach ( Node child in GetChildren() )
				if ( child is Node3D node3D )
					node3D.Position = localTip;
		}

		private BodyHandle? ResolveExcludeHandle()
		{
			switch ( ExcludeBody )
			{
			case BepuCharacterBody3D character:
				return character.Handle;
			case BepuRigidBody3D rigid:
				return rigid.Handle;
			case BepuProjectile3D projectile:
				return projectile.Handle;
			default:
				return null;
			}
		}
	}
}
