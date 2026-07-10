// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;

namespace Physics
{
	/// <summary>
	/// Immobile level geometry. Built once from either a convex <see cref="CollisionShape3D"/>
	/// (Box/Sphere/Capsule/Cylinder - cheap, preferred for small/simple pieces) or a
	/// <see cref="MeshInstance3D"/> (arbitrary triangle soup).
	///
	/// If this geometry ever needs to move (doors, platforms, elevators), don't reparent or
	/// retransform this node - use a kinematic body instead; statics are baked into the
	/// broadphase and are not meant to move.
	/// </summary>
	[GlobalClass]
	public partial class BepuStaticBody3D : BepuBody3D
	{
		[ExportGroup( "Source" )]
		[Export] private CollisionShape3D? _convexShape;
		[Export] private MeshInstance3D? _meshShape;

		public StaticHandle Handle { get; private set; }

		protected override void OnRegister()
		{
			BuiltShape built;

			if ( _convexShape != null )
			{
				built = GodotShapeConverter.FromCollisionShape3D( World, _convexShape, mass: 0f );
			}
			else if ( _meshShape != null )
			{
				built = GodotShapeConverter.FromMeshInstance3D( World, _meshShape, GlobalTransform.Basis.Scale );
			}
			else
			{
				GD.PushError( $"{Name}: BepuStaticBody3D needs either a CollisionShape3D or MeshInstance3D assigned." );
				return;
			}

			PhysicsTransform pose = GodotShapeConverter.ToPhysicsTransform( GlobalTransform, built.LocalOffset );
			Handle = World.Core.AddStatic( pose, built.Handle, (uint)Layer, (uint)Mask, OwnerId );
		}

		protected override void OnUnregister() => World.Core.RemoveStatic( Handle );
	}
}
