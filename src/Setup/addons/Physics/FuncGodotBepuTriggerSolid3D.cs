// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;
using System.Collections.Generic;
using Zenjex;

namespace Physics
{
	/// <summary>
	/// node_class target for func_godot Solid Class trigger entities (the brush-shaped
	/// equivalent of Quake's trigger_once / trigger_multiple / trigger_push / trigger_hurt).
	///
	/// Works exactly like <see cref="FuncGodotBepuStaticBody3D"/> - inherits StaticBody3D purely
	/// so func_godot's "node is CollisionObject3D" check keeps generating CollisionShape3D
	/// children from the brush geometry - but every shape is registered with
	/// <see cref="PhysicsObjectKind.Trigger"/> instead of Solid, so nothing physically collides
	/// with it; it only reports overlaps.
	///
	/// FGD class_properties this entity understands (set via the Solid Class .tres, applied
	/// through func_godot's <c>_func_godot_apply_properties</c> hook - see
	/// entity_assembler.gd):
	///   - "target"    (string) - Godot group name notified via <see cref="Triggered"/> when fired.
	///   - "once"      (bool)   - if true, the trigger disables itself after firing once.
	///   - "activator_mask" (int) - collision mask; only bodies whose owner is in this mask's
	///     layer are considered valid activators (defaults to Character layer).
	/// </summary>
	[GlobalClass]
	public partial class FuncGodotBepuTriggerSolid3D : BepuStaticBody3D, IPhysicsCollisionListener
	{
		[ExportGroup( "Behavior" )]
		[Export] public bool Once { get; set; } = false;
		[Export] public string Target { get; set; } = "";

		[Signal] public delegate void TriggeredEventHandler( Node3D activator );

		[Inject]
		private IPhysicsWorld _world = null!;

		private int _ownerId;
		private readonly List<StaticHandle> _handles = new List<StaticHandle>();
		private bool _spent;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void _Ready()
		{
			_ownerId = _world.RegisterOwner( this );

			foreach ( Node child in GetChildren() )
			{
				if ( child is CollisionShape3D collisionShape && collisionShape.Shape != null )
					RegisterShape( collisionShape );
			}
		}

		public override void _ExitTree()
		{
			foreach ( StaticHandle handle in _handles )
				_world.Core.RemoveStatic( handle );

			_handles.Clear();
			_world.UnregisterOwner( _ownerId );
		}

		private void RegisterShape( CollisionShape3D collisionShape )
		{
			BuiltShape built = GodotShapeConverter
				.FromCollisionShape3D( _world, collisionShape, mass: 0f );

			PhysicsTransform pose = GodotShapeConverter
				.ToPhysicsTransform( collisionShape.GlobalTransform, built.LocalOffset );

			StaticHandle handle = _world.Core.AddStatic(
				pose: pose,
				shape: built.Handle,
				layer: (uint)Layer,
				mask: (uint)Mask,
				 ownerId: _ownerId,
				kind: PhysicsObjectKind.Trigger
			);

			_handles.Add( handle );
		}

		/// <summary>func_godot calls this automatically after spawning, with the entity's FGD key/value properties. Must stay public and keep this exact snake_case name - func_godot's entity_assembler.gd finds it via Node.HasMethod("_func_godot_apply_properties") from GDScript, which only sees public C# members.</summary>
		public void _func_godot_apply_properties( Godot.Collections.Dictionary properties )
		{
			if ( properties.TryGetValue( "target", out Variant targetValue ) )
				Target = targetValue.AsString();

			if ( properties.TryGetValue( "once", out Variant onceValue ) )
				Once = onceValue.AsBool();
		}

		void IPhysicsCollisionListener.OnPhysicsBodyEntered( BepuBody3D other )
		{
			if ( _spent )
				return;

			Fire( other );

			if ( Once )
				_spent = true;
		}

		void IPhysicsCollisionListener.OnPhysicsBodyExited( BepuBody3D other ) { }

		private void Fire( Node3D activator )
		{
			EmitSignal( SignalName.Triggered, activator );

			if ( !string.IsNullOrEmpty( Target ) )
			{
				foreach ( Node node in GetTree().GetNodesInGroup( Target ) )
				{
					if ( node.HasMethod( "_on_bepu_trigger_fired" ) )
						node.Call( "_on_bepu_trigger_fired", this, activator );
				}
			}
		}
	}
}
