// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using Zenjex;
using Dictionary = Godot.Collections.Dictionary;
using Ex = Framework.Common.Extensions.NodeExtensions;
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

		public int PhysicsId { get; private set; }

		private IPhysicsWorld _physicsWorld;
		protected IPhysicsWorld World => _physicsWorld;

		[Inject]
		private void Construct( IPhysicsWorld world ) => Ex.EditorSafe( () =>
		{
			_physicsWorld = world;
		} );

		public override void _EnterTree() => Ex.EditorSafe( () =>
		{
			DiContainer.Instance.Inject( this );
		} );

		public override void _Ready() => Ex.EditorSafe( () =>
		{
			PhysicsId = _physicsWorld.RegisterOwner( this );
			OnRegister();
		} );

		public override void _ExitTree() => Ex.EditorSafe( () =>
		{
			OnUnregister();
			World.UnregisterOwner( PhysicsId );
		} );

		/// <summary>Register this component's body/static with <see cref="World"/>.<see cref="IPhysicsWorld.Core"/> here.</summary>
		protected abstract void OnRegister();

		/// <summary>Remove this component's body/static from <see cref="World"/>.<see cref="IPhysicsWorld.Core"/> here.</summary>
		protected abstract void OnUnregister();

		#region Godot Callbacks

		public override void _Process( double delta ) => Ex.EditorSafe( () =>
		{
			base._Process( delta );
		} );

		public override void _PhysicsProcess( double delta ) => Ex.EditorSafe( () =>
		{
			base._PhysicsProcess( delta );
		} );

		public override void _Input( InputEvent @event ) => Ex.EditorSafe( () =>
		{
			base._Input( @event );
		} );

		public override void _ShortcutInput( InputEvent @event ) => Ex.EditorSafe( () =>
		{
			base._ShortcutInput( @event );
		} );

		public override void _UnhandledInput( InputEvent @event ) => Ex.EditorSafe( () =>
		{
			base._UnhandledInput( @event );
		} );

		public override void _UnhandledKeyInput( InputEvent @event ) => Ex.EditorSafe( () =>
		{
			base._UnhandledKeyInput( @event );
		} );

		public override void _Notification( int what ) => Ex.EditorSafe( () =>
		{
			base._Notification( what );
		} );

		public override void _ValidateProperty( Dictionary property ) => Ex.EditorSafe( () =>
		{
			base._ValidateProperty( property );
		} );

		#endregion
	}
}
