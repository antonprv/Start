// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Logger;
using Framework.Physics;
using Godot;

namespace Physics
{
	/// <summary>
	/// A regular dynamic physics object (crates, debris, pickups that should tumble around).
	/// Mass/shape come from a child <see cref="CollisionShape3D"/>; gravity, damping and solver
	/// response are handled entirely inside Core - this component's only per-frame job is to
	/// copy the resulting pose back onto the Godot transform for rendering.
	/// </summary>
	[GlobalClass]
	public partial class BepuRigidBody3D : BepuBody3D
	{
		[ExportGroup( "Source" )]
		[Export] private CollisionShape3D _shapeSource = null!;
		[Export] private float _mass = 1f;

		[ExportGroup( "CCD" )]
		[Export] private bool _continuousDetection;

		public BodyHandle Handle { get; private set; }
		private Vector3 _localOffset;

		protected override void OnRegister()
		{
			if ( _shapeSource == null )
			{
				GameLogger
					.LogError( $"{Name}: BepuRigidBody3D requires a" +
					$" CollisionShape3D assigned to ShapeSource." );
				return;
			}

			BuiltShape built = GodotShapeConverter
				.FromCollisionShape3D( World, _shapeSource, _mass );
			_localOffset = built.LocalOffset;

			PhysicsTransform pose = GodotShapeConverter
				.ToPhysicsTransform( GlobalTransform, _localOffset );
			Handle = World.Core.AddDynamicBody( 
				pose: pose, 
				shape: built.Handle, 
				mass: _mass, 
				layer: (uint)Layer, 
				mask: (uint)Mask, 
				ownerId: OwnerId, 
				kind: PhysicsObjectKind.Solid, 
				continuousDetection: _continuousDetection 
			);
		}

		protected override void OnUnregister() => World.Core.RemoveBody( Handle );

		public override void _PhysicsProcess( double delta )
		{
			if ( !World.Core.BodyExists( Handle ) )
				return;

			PhysicsTransform pose = World.Core.GetBodyPose( Handle );
			Quaternion orientation = GodotShapeConverter.ToGodot( pose.Orientation );

			// Shapes with a non-zero LocalOffset (convex hulls) are positioned by their own
			// centroid in Core; undo that offset so the node's origin stays where the source
			// CollisionShape3D actually was, not the hull's centroid.
			Vector3 worldOffset = orientation * _localOffset;
			Vector3 nodeOrigin = GodotShapeConverter.ToGodot( pose.Position ) - worldOffset;

			GlobalTransform = new Transform3D( new Basis( orientation ), nodeOrigin );
		}

		/// <summary>Linear velocity in world space, read/write.</summary>
		public Vector3 Velocity
		{
			get => GodotShapeConverter.ToGodot( World.Core.GetLinearVelocity( Handle ) );
			set => World.Core.SetLinearVelocity( Handle, GodotShapeConverter.ToNumerics( value ) );
		}

		public void ApplyImpulse( Vector3 impulse, Vector3 worldPoint )
		{
			Vector3 offset = worldPoint - GlobalPosition;

			World.Core.ApplyImpulse( 
				handle: Handle, 
				impulse: GodotShapeConverter.ToNumerics( impulse ), 
				offset: GodotShapeConverter.ToNumerics( offset ) 
			);
		}
	}
}
