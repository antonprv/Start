// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if TOOLS

using Framework.Common.FuncGodot;
using Godot;
using Physics;

using Dictionary = Godot.Collections.Dictionary;
using GEngine = Godot.Engine;

namespace Engine.Gameplay.Entities
{
	[Tool]
	public partial class MovingPlatform : BepuAnimatableBody3D, IEntity
	{
		[Export] public float MoveDistance { get; set; } = 2.0f;
		[Export] public float MoveTime { get; set; } = 2.0f;
		[Export] public Vector3 MoveDirection { get; set; } = Vector3.Up;

		public Dictionary FuncGodotProperties { get; set; } = new Dictionary();

		private Vector3 _startPosition;
		private Vector3 _endPosition;
		private Tween _platformTween;

		public void _FuncGodotApplyProperties( Dictionary entityProperties )
		{
			MoveDistance = entityProperties[ "move_distance" ].As<float>();
			MoveTime = entityProperties[ "move_time" ].As<float>();
			MoveDirection = entityProperties[ "move_direction" ].As<Vector3>();
		}

		public void _FuncGodotBuildComplete()
		{
			// post-initialize work
		}

		protected override void OnRegister()
		{
			if (!GEngine.IsEditorHint())
			{
				base.OnRegister();
				_startPosition = GlobalPosition;
				_endPosition = _startPosition + MoveDirection.Normalized() * MoveDistance;
				StartMovement();
			}
		}

		private void StartMovement()
		{
			_platformTween = CreateTween();
			_platformTween.SetLoops();
			_platformTween.TweenProperty( this, "global_position", _endPosition, MoveTime );
			_platformTween.TweenProperty( this, "global_position", _startPosition, MoveTime );
		}

		#region Godot Callbacks

		public override void _EnterTree()
		{
			if ( !GEngine.IsEditorHint() )
				base._EnterTree();
		}

		public override void _ExitTree()
		{
			if ( !GEngine.IsEditorHint() )
				base._ExitTree();
		}

		public override void _Ready()
		{
			if ( !GEngine.IsEditorHint() )
				base._Ready();
		}

		protected override void OnUnregister()
		{
			if ( !GEngine.IsEditorHint() )
				base.OnUnregister();
		}

		public override void _PhysicsProcess( double delta )
		{
			if ( !GEngine.IsEditorHint() )
				base._PhysicsProcess( delta );
		}

		#endregion
	}

#endif
}