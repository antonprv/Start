// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using System;
using Zenjex;

namespace Physics
{
	/// <summary>
	/// Common plumbing shared by every Bepu-backed physics component: collision layer/mask
	/// export fields, DI injection of <see cref="IPhysicsWorld"/>, and owner registration so the
	/// world can resolve "which Node3D does this collidable belong to" for overlap events.
	/// </summary>
	[GlobalClass]
	public abstract partial class BepuBody3D : Node3D
	{
		[ExportGroup( "Collision" )]
		[Export] public CollisionLayer Layer { get; set; } = CollisionLayer.World;
		[Export] public CollisionLayer Mask { get; set; } = CollisionLayer.All;

		private IPhysicsWorld _physicsWorld;
		protected IPhysicsWorld World => _physicsWorld;
		protected int OwnerId { get; private set; }

		[Inject]
		private void Construct( IPhysicsWorld world ) => _physicsWorld = world;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void _Ready()
		{
			OwnerId = _physicsWorld.RegisterOwner( this );
			OnRegister();
		}

		public override void _ExitTree()
		{
			OnUnregister();
			World.UnregisterOwner( OwnerId );
		}

		/// <summary>Register this component's body/static with <see cref="World"/>.<see cref="IPhysicsWorld.Core"/> here.</summary>
		protected abstract void OnRegister();

		/// <summary>Remove this component's body/static from <see cref="World"/>.<see cref="IPhysicsWorld.Core"/> here.</summary>
		protected abstract void OnUnregister();
	}
}
