// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Common.Draw;
using Framework.Logger;
using Godot;
using Physics;

namespace Engine.Common.Physics
{
	public abstract partial class TriggerVolume : BepuTriggerAreaSimple
	{
		[ExportGroup( "Logic" )]
		[Export] public bool RegisterCollisions { get; set; }

		[ExportGroup( "Visual Debug" )]
		[Export] private Color _normalColor = new Color( 0, 1, 0 );       // green
		[Export] private Color _triggerEnterColor = new Color( 0, 0, 1 ); // blue
		[Export] private Color _triggerExitColor = new Color( 1, 0, 0 );  // red
		[Export] private float _flashDuration = 0.5f;

		[ExportGroup( "Logging" )]
		[Export] public bool LogCollision { get; set; }
		[Export] public bool DrawDebugCollision { get; set; }

		private DebugHandle _debugDrawHandle;

		public override void _Ready()
		{
			base._Ready();
			InitializeInternal();
		}

		private void InitializeInternal()
		{
			if ( DrawDebugCollision )
			{
				if ( ShapeSource.Shape is BoxShape3D shape )
				{
					_debugDrawHandle = DrawDebugRuntime
							.DrawWireCube( ShapeSource.GlobalPosition, shape.Size, _normalColor );
				}
				else
				{
					DrawDebugCollision = false;
					GameLogger.LogError( $"{typeof( CollisionShape3D )}'s shape is not {typeof( BoxShape3D )}." +
						$" Displaying realtime debug info is disabled." );
				}
			}

			Initialize();
		}

		protected override void OnBodyEnter()
		{
			base.OnBodyEnter();
			HandleBodyEnter( Target );
		}

		protected override void OnBodyExit()
		{
			base.OnBodyExit();
			HandleBodyExit( Target );
		}

		public abstract void Initialize();
		protected abstract void OnBodyEnter( Node3D body );
		protected abstract void OnBodyExit( Node3D body );

		private void HandleBodyEnter( Node3D body )
		{
			if ( !RegisterCollisions )
				return;

			OnBodyEnter( body );

			if ( LogCollision )
				GameLogger.LogInfo( $"{body} entered!" );

			if ( DrawDebugCollision )
				DrawDebugRuntime.Flash(
						handle: _debugDrawHandle,
						flashColor: _triggerEnterColor,
						duration: _flashDuration,
						returnColor: _normalColor
					);
		}

		private void HandleBodyExit( Node3D body )
		{
			if ( !RegisterCollisions )
				return;

			OnBodyExit( body );

			if ( LogCollision )
				GameLogger.LogInfo( $"{body} exited!" );

			if ( DrawDebugCollision )
				DrawDebugRuntime.Flash(
						handle: _debugDrawHandle,
						flashColor: _triggerExitColor,
						duration: _flashDuration,
						returnColor: _normalColor
					);
		}
	}
}
