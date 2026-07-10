// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;

namespace Physics
{
	/// <summary>
	/// node_class target for func_godot Point Class physics props (the brush-less equivalent of
	/// Quake's misc_model / func_physbox-style entities) - place a point entity in the map editor
	/// and it spawns as a Bepu dynamic rigid body, sized/weighted from FGD class_properties.
	///
	/// Unlike Solid Classes, Point Classes never get brush-generated CollisionShape3D children -
	/// there's no brush to build one from - so this component builds its own box shape from the
	/// "size" property instead of reading a child CollisionShape3D like
	/// <see cref="BepuRigidBody3D"/> does. If you need a prop with a specific mesh/visual, prefer
	/// wiring the FGD Point Class's "Scene File" field to a hand-built scene containing a
	/// <see cref="BepuRigidBody3D"/> instead of using this node_class directly - this component is
	/// for quick, mesh-less placeholders and gameplay testing.
	///
	/// FGD class_properties this entity understands, applied via func_godot's
	/// <c>_func_godot_apply_properties</c> hook (see entity_assembler.gd):
	///   - "mass" (float)   - overrides the exported Mass.
	///   - "size" (Vector3) - overrides the exported Size (box half... no, full extents, matching BoxShape3D.Size).
	/// </summary>
	[GlobalClass]
	public partial class FuncGodotBepuPhysicsProp3D : BepuBody3D
	{
		[ExportGroup( "Shape" )]
		[Export] public Vector3 Size { get; set; } = new Vector3( 0.5f, 0.5f, 0.5f );
		[Export] public float Mass { get; set; } = 5f;

		[ExportGroup( "CCD" )]
		[Export] public bool ContinuousDetection { get; set; } = false;

		public BodyHandle Handle { get; private set; }

		/// <summary>
		/// func_godot calls this before <see cref="OnRegister"/> ever runs? No - properties are
		/// applied by entity_assembler.gd AFTER the node is added and BepuBody3D._Ready() (which
		/// calls OnRegister) has already fired, since Godot node lifecycle is add-to-tree first,
		/// property application second. That means by the time this fires, the body already
		/// exists with the exported defaults. Re-registering here with the corrected size/mass is
		/// simplest and only happens once at map-build/spawn time, not per-frame.
		/// </summary>
		public void _func_godot_apply_properties( Godot.Collections.Dictionary properties )
		{
			bool needsRebuild = false;

			if ( properties.TryGetValue( "mass", out Variant massValue ) )
			{
				Mass = massValue.AsSingle();
				needsRebuild = true;
			}

			if ( properties.TryGetValue( "size", out Variant sizeValue ) )
			{
				Size = sizeValue.AsVector3();
				needsRebuild = true;
			}

			if ( needsRebuild && World != null )
			{
				OnUnregister();
				OnRegister();
			}
		}

		protected override void OnRegister()
		{
			ShapeHandle shapeHandle = World.Core.AddBoxShape( GodotShapeConverter.ToNumerics( Size ) );
			PhysicsTransform pose = GodotShapeConverter.ToPhysicsTransform( GlobalTransform );
			Handle = World.Core.AddDynamicBody( pose, shapeHandle, Mass, (uint)Layer, (uint)Mask, OwnerId, PhysicsObjectKind.Solid, ContinuousDetection );
		}

		protected override void OnUnregister() => World.Core.RemoveBody( Handle );

		public override void _PhysicsProcess( double delta )
		{
			if ( !World.Core.BodyExists( Handle ) )
				return;

			PhysicsTransform pose = World.Core.GetBodyPose( Handle );
			Quaternion orientation = GodotShapeConverter.ToGodot( pose.Orientation );
			Vector3 position = GodotShapeConverter.ToGodot( pose.Position );
			GlobalTransform = new Transform3D( new Basis( orientation ), position );
		}
	}
}
