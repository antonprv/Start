// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

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
	/// Each physics tick it sweeps a sphere from this node's position along local -Z by up to
	/// <see cref="SpringLength"/>, then repositions every direct Node3D child to
	/// <c>(0, 0, -CurrentLength)</c> - exactly like the native node. Typical use: parent this
	/// under the player, put a Camera3D as its only child.
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
			Vector3 direction = -GlobalTransform.Basis.Z;
			BodyHandle? exclude = ResolveExcludeHandle();

			ShapeCastResult result = _world.Core.SweepSphereCast(
				GodotShapeConverter.ToNumerics( origin ),
				GodotShapeConverter.ToNumerics( direction ),
				SpringLength,
				Radius,
				(uint)Layer,
				(uint)Mask,
				exclude );

			IsColliding = result.Hit;
			CurrentLength = result.Hit ? Mathf.Max( 0f, result.Distance - Margin ) : SpringLength;

			Vector3 localTip = new Vector3( 0f, 0f, -CurrentLength );
			foreach ( Node child in GetChildren() )
			{
				if ( child is Node3D node3D )
					node3D.Position = localTip;
			}
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
