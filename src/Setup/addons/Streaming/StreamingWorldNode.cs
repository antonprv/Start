// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Logger;
using Framework.Streaming;
using Godot;
using Setup.addons.Streaming;

namespace Streaming.Autoload
{
    /// <summary>
    /// Add one instance of this to your scene tree (commonly as an autoload, e.g.
    /// "/root/StreamingWorld"). Owns the engine-agnostic <see cref="Framework.Streaming.StreamingWorld"/>,
    /// feeds it the active Camera3D's position every tick, and drives its update loop.
    ///
    /// Runs on a throttled cadence rather than every _Process call - streaming decisions don't
    /// need to be render-framerate accurate, and re-sorting the resource list every single frame
    /// is wasted work once you have a few hundred streamable props in a level.
    /// </summary>
    public partial class StreamingWorldNode : Node, IStreamingWorld
    {
        [ExportGroup( "Budget" )]
        [Export] private long _bytesPerTick = 2 * 1024 * 1024;
        [Export] private int _maxResourceUpdatesPerTick = 8;
        [Export] private float _fullDetailDistance = 8f;
        [Export] private float _minDetailDistance = 60f;
        /// <summary>Total resident streaming memory allowed, in megabytes. 0 means uncapped.</summary>
        [Export] private long _memoryBudgetMb;

        [ExportGroup( "Scheduling" )]
        [Export] private float _tickIntervalSeconds = 0.1f;

        public StreamingWorld Core { get; private set; } = null!;

        private Camera3D? _activeCamera;
        private double _timeSinceLastTick;

        public override void _Ready()
        {
            var budget = new StreamingBudget(
                _bytesPerTick,
                _maxResourceUpdatesPerTick,
                _fullDetailDistance,
                _minDetailDistance,
                memoryBudgetBytes: _memoryBudgetMb > 0 ? _memoryBudgetMb * 1024 * 1024 : long.MaxValue );

            Core = new StreamingWorld( new GodotChunkDataSource(), budget );
            GameLogger.LogInfo( "StreamingWorld initialized." );
        }

        public override void _Process( double delta )
        {
            _timeSinceLastTick += delta;
            if ( _timeSinceLastTick < _tickIntervalSeconds )
                return;
            _timeSinceLastTick = 0;

            _activeCamera ??= GetViewport().GetCamera3D();
            if ( _activeCamera == null )
                return;

            Vector3 position = _activeCamera.GlobalPosition;
            Core.SetViewer( new StreamingViewer(
                new System.Numerics.Vector3( position.X, position.Y, position.Z ),
                _activeCamera.Fov * Mathf.Pi / 180f ) );

            Core.Update();
        }

        public override void _ExitTree() => Core?.Dispose();
    }
}
