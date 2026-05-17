// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Extensions.Logging;
using Code.Services.Input;

using FastMath;
using Godot;
using System;
using ZenjexGodot;

namespace Code.Components
{
	public partial class Mover : CharacterBody3D
	{
		[ExportGroup( "Movement" )]
		[Export] private float _moveSpeed = 5f;
		[Export] private float _rotationSpeed = 6f;

		[ExportGroup( "References" )]
		[Export] private MeshInstance3D _playerMesh;

		[Inject] private readonly IInputService _inputService;
		[Inject] private readonly IGameLog _log;

		private Vector3 _moveDirection;
		private Vector2 _inputDirection;
		private bool _wantsToJump;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void _UnhandledKeyInput( InputEvent inputEvent )
		{
			BindInputs( inputEvent );
			HandleInputs();
		}

		public override void _PhysicsProcess( double delta )
		{
			HandleMovement();
			HandleRotation( delta );
			MoveAndSlide();
		}

		private void BindInputs( InputEvent inputEvent )
		{
			_inputDirection = _inputService.GetInputVector();
			_wantsToJump = _inputService.GetJumpState( inputEvent );
		}

		private void HandleInputs() =>
			_moveDirection = new Vector3( _inputDirection.X, 0, _inputDirection.Y ).Normalized();

		private void HandleMovement()
		{
			if ( _moveDirection.IsNearlyEqual( Vector3.Zero ) )
			{
				Velocity = Vector3.Zero;
				return;
			}

			Velocity = _moveDirection * _moveSpeed;
		}

		private void HandleRotation( double delta )
		{
			if ( _playerMesh == null || _moveDirection.IsNearlyEqual( Vector3.Zero ) )
				return;

			float targetAngle = FMath.FastAtan2( -_moveDirection.X, -_moveDirection.Z );

			Quaternion targetRotation = new Quaternion( Vector3.Up, targetAngle );
			Quaternion currentRotation = _playerMesh.Quaternion;
			Quaternion smoothRotation = currentRotation.FastSlerp( targetRotation, (float)( _rotationSpeed * delta ) );

			_playerMesh.Quaternion = smoothRotation;
		}
	}
}