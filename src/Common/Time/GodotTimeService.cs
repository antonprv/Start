// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Common.Time
{
    public class GodotTimeService : ITimeService
    {
        #region State

        private bool _paused;
        private float _delta;
        private float _unscaledDelta;

        #endregion

        #region ITimeService

        public float DeltaTime => _paused ? 0f : _delta;
        public float DeltaAt60FPS => _paused ? 0f : _delta / 0.016f;
        public float DeltaAt100FPS => _paused ? 0f : _delta / 0.01f;
        public float DeltaAtOffset => _paused ? 0f : _delta + 0.01f;
        public float UnscaledDeltaTime => _paused ? 0f : _unscaledDelta;
        public DateTime UtcNow => DateTime.UtcNow;

        public void StopTime() => _paused = true;
        public void StartTime() => _paused = false;

        #endregion

        #region Feed

        public void Tick( double delta )
        {
            _delta = (float)delta;

            double timeScale = Engine.TimeScale;
            _unscaledDelta = timeScale > 0.0
              ? (float)( delta / timeScale )
              : _delta;
        }

        #endregion
    }
}
