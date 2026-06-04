using System;
using EcosDelLaberinto.Data;

namespace EcosDelLaberinto.Gameplay.Time
{
    /// <summary>
    /// Default fixed-step clock. A real-time accumulator emits a discrete tick every
    /// <see cref="TickInterval"/> seconds. Capped per-frame to avoid the "spiral of death" after a
    /// long stall (e.g. window unfocused).
    /// </summary>
    public sealed class TickService : ITickService
    {
        private const int MaxTicksPerFrame = 5;

        private readonly float _interval;
        private float _accumulator;

        public event Action<int> Ticked;

        public int CurrentTick { get; private set; }
        public bool IsPaused { get; set; }
        public float TickInterval => _interval;

        public TickService(GameConfig config)
        {
            _interval = config.TickInterval;
        }

        public void Advance(float deltaTime)
        {
            if (IsPaused)
            {
                return;
            }

            _accumulator += deltaTime;
            var processed = 0;

            while (_accumulator >= _interval && processed < MaxTicksPerFrame)
            {
                _accumulator -= _interval;
                Ticked?.Invoke(CurrentTick);
                CurrentTick++;
                processed++;
            }

            // Drop backlog if we hit the cap, otherwise the sim would fast-forward unfairly.
            if (processed >= MaxTicksPerFrame && _accumulator > _interval)
            {
                _accumulator = 0f;
            }
        }

        public void ResetTicks()
        {
            CurrentTick = 0;
            _accumulator = 0f;
        }
    }
}
