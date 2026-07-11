// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Physics;
using Godot;

namespace Physics
{
	/// <summary>
	/// Small, fast-moving body (bullets, rockets, thrown grenades). Each tick this drives
	/// <see cref="Framework.Physics.PhysicsWorld.SweepProjectile"/> with its own
	/// Velocity * delta rather than leaning on the solver - see that method's doc comment for why
	/// (short version: a solver contact means "bounce/rest", a sweep hit means "stop here, tell
	/// me exactly what and where, once" - the latter is what Quake-style projectiles want, and it
	/// can't tunnel regardless of speed since the sweep itself IS the movement).
	///
	/// On first hit it calls <see cref="IProjectileHitListener.OnProjectileHit"/> on whichever
	/// ancestor/owner script implements it (set via <see cref="HitListener"/>, or auto-resolved
	/// from the parent if left null) and queues itself for deletion.
	/// </summary>
	[GlobalClass]
	public partial class BepuProjectile3D : BepuBody3D
	{
		[ExportGroup( "Shape" )]
		[Export] private float _radius = 0.05f;

		[ExportGroup( "Behavior" )]
		[Export] private float _maxLifetimeSeconds = 5f;
		[Export] private bool _destroyOnHit = true;
		[Export] private bool _applyGravity;
		[Export] private float _gravity = -20f;

		public BodyHandle Handle { get; private set; }
		public Vector3 Velocity;
		public IProjectileHitListener? HitListener;

		private float _lifetime;
		private bool _resolved;

		protected override void OnRegister()
		{
			HitListener ??= GetParent() as IProjectileHitListener;

			ShapeHandle shapeHandle = World.Core.AddSphereShape( _radius );
			PhysicsTransform pose = PhysicsTransform
				.FromPosition( GodotShapeConverter.ToNumerics( GlobalPosition ) );
			
			Handle = World.Core.AddKinematicBody( 
				pose: pose, 
				shape: shapeHandle, 
				layer: (uint)Layer, 
				mask: (uint)Mask, 
				ownerId: OwnerId, 
				kind: PhysicsObjectKind.Projectile 
			);
		}

		protected override void OnUnregister() => World.Core.RemoveBody( Handle );

		public override void _PhysicsProcess( double delta )
		{
			if ( _resolved )
				return;

			float dt = (float)delta;
			_lifetime += dt;

			if ( _applyGravity )
				Velocity += new Vector3( 0f, _gravity, 0f ) * dt;

			ProjectileSweepResult result = World.Core.SweepProjectile(
				Handle,
				GodotShapeConverter.ToNumerics( GlobalPosition ),
				GodotShapeConverter.ToNumerics( Velocity ),
				dt, _radius, (uint)Layer, (uint)Mask );

			GlobalPosition = GodotShapeConverter.ToGodot( result.Position );

			if ( result.Hit )
			{
				_resolved = true;
				Node3D? hitOwner = World.GetOwner( result.HitOwnerId );
				HitListener?.OnProjectileHit( 
					GodotShapeConverter.ToGodot( result.Point ), 
					GodotShapeConverter.ToGodot( result.Normal ), 
					hitOwner 
				);

				if ( _destroyOnHit )
					QueueFree();
				return;
			}

			World.Core.SetBodyPose( 
				Handle, 
				PhysicsTransform
					.FromPosition( GodotShapeConverter.ToNumerics( GlobalPosition ) )
			);

			if ( _lifetime >= _maxLifetimeSeconds )
			{
				_resolved = true;
				QueueFree();
			}
		}
	}
}
