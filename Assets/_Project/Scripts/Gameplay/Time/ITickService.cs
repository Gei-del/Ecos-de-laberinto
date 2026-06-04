using System;

namespace EcosDelLaberinto.Gameplay.Time
{
    /// <summary>
    /// Fixed-step clock that drives the deterministic simulation. Movement, echo recording and
    /// echo playback all advance on <see cref="Ticked"/> so they stay perfectly in sync
    /// regardless of frame rate (critical for fair speedruns and replay accuracy).
    /// </summary>
    public interface ITickService
    {
        /// <summary>Fired once per fixed tick with the current tick index (0-based within a loop).</summary>
        event Action<int> Ticked;

        int CurrentTick { get; }
        bool IsPaused { get; set; }
        float TickInterval { get; }

        /// <summary>Advance the accumulator by real time; raises Ticked as many times as needed.</summary>
        void Advance(float deltaTime);

        /// <summary>Reset the tick counter to 0 (start of a new loop).</summary>
        void ResetTicks();
    }
}
