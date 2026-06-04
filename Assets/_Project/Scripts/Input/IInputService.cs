using EcosDelLaberinto.Domain;

namespace EcosDelLaberinto.Input
{
    /// <summary>
    /// Abstraction over raw input so gameplay reads intent, not devices. Enables rebinding,
    /// Steam Deck controller support and deterministic playback (an echo is just a recorded
    /// stream of these intents).
    /// </summary>
    public interface IInputService
    {
        /// <summary>Desired move direction this frame (held), already snapped to the grid.</summary>
        GridDirection MoveDirection { get; }
        bool InteractHeld { get; }
        bool AbilityPressed { get; }
        bool RestartPressed { get; }
        bool PausePressed { get; }

        void Poll();

        /// <summary>Clears latched one-shot actions (ability/restart/pause) after a tick consumes them.</summary>
        void ConsumeOneShots();
    }
}
