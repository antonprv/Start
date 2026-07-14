// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;
using System.Collections.Generic;

namespace Physics
{
	/// <summary>
	/// Drop-in <c>node_class</c> target for func_godot solid class definitions. Assign this
	/// class's name to a <c>FuncGodotFGDSolidClass</c>'s Node Class field in the FGD editor
	/// (worldspawn, func_wall, whatever) and func_godot will do exactly what it already does for
	/// any other CollisionObject3D-derived class: bake one CollisionShape3D per brush (Convex) or
	/// one combined CollisionShape3D (Concave) as children, according to that entity's Collision
	/// Build settings - see func_godot's entity_assembler.gd, generate_solid_entity_node(). This
	/// is a fully supported, first-class extension point, not a hack.
	///
	/// func_godot never needs to know Bepu (or even this addon) exists: it just sees a
	/// CollisionObject3D and does its normal thing. This class then reads those same stock
	/// CollisionShape3D/Shape3D nodes/resources back out at _Ready() (i.e. after the map has been
	/// built and saved into the scene) and turns each one into a physics static via
	/// <see cref="GodotShapeConverter"/> + <see cref="IPhysicsWorld"/>.
	///
	/// Inherits StaticBody3D (rather than a plain Node3D, like the other components in this
	/// addon) specifically so func_godot's "node is CollisionObject3D" check keeps working
	/// unmodified. By default this also disables the native Godot collision response on itself
	/// (see <see cref="DisableNativePhysics"/>) so you don't end up with both Godot's
	/// PhysicsServer3D and our physics simulating the same walls - the CollisionShape3D children
	/// stay in the tree (useful for NavigationServer baking, editor gizmos, occlusion, etc), they
	/// just don't collide with anything through Godot's own server.
	/// </summary>
	[GlobalClass]
	public partial class FuncGodotBepuStaticBody3D : BepuStaticBody3D
	{
		private int _ownerId;
		private readonly List<StaticHandle> _handles = new List<StaticHandle>();

		public override void _Ready()
		{
			_ownerId = World.RegisterOwner( this );

			// func_godot bakes one CollisionShape3D per brush (Convex mode) or a single combined
			// one (Concave mode) as direct children.
			foreach ( Node child in GetChildren() )
			{
				if ( child is CollisionShape3D collisionShape && collisionShape.Shape != null )
					RegisterShape( collisionShape );
			}
		}

		public override void _ExitTree()
		{
			foreach ( StaticHandle handle in _handles )
				World.Core.RemoveStatic( handle );

			_handles.Clear();
			World.UnregisterOwner( _ownerId );
		}

		private void RegisterShape( CollisionShape3D collisionShape )
		{
			BuiltShape built = GodotShapeConverter
				.FromCollisionShape3D( World, collisionShape, mass: 0f );

			PhysicsTransform pose = GodotShapeConverter
				.ToPhysicsTransform( collisionShape.GlobalTransform, built.LocalOffset );

			StaticHandle handle = World.Core.AddStatic(
				pose: pose,
				shape: built.Handle,
				layer: (uint)Layer,
				mask: (uint)Mask,
				ownerId: _ownerId,
				kind: PhysicsObjectKind.Solid
			);

			_handles.Add( handle );
		}
	}
}
