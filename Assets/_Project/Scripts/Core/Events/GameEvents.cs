using EcosDelLaberinto.Domain;

namespace EcosDelLaberinto.Core.Events
{
    // ---------------------------------------------------------------------------------------------
    // Gameplay loop events. Every system reacts to these instead of polling each other.
    // ---------------------------------------------------------------------------------------------

    public readonly struct LevelLoadedEvent : IGameEvent
    {
        public readonly string LevelId;
        public readonly int CrystalsTotal;
        public LevelLoadedEvent(string levelId, int crystalsTotal)
        {
            LevelId = levelId;
            CrystalsTotal = crystalsTotal;
        }
    }

    /// <summary>Raised every time a new loop begins (fresh player + echoes replaying).</summary>
    public readonly struct LoopStartedEvent : IGameEvent
    {
        public readonly int LoopIndex;
        public readonly int ActiveEchoes;
        public LoopStartedEvent(int loopIndex, int activeEchoes)
        {
            LoopIndex = loopIndex;
            ActiveEchoes = activeEchoes;
        }
    }

    public readonly struct PlayerDiedEvent : IGameEvent
    {
        public readonly string Cause;
        public PlayerDiedEvent(string cause) => Cause = cause;
    }

    public readonly struct CrystalCollectedEvent : IGameEvent
    {
        public readonly int Collected;
        public readonly int Total;
        public CrystalCollectedEvent(int collected, int total)
        {
            Collected = collected;
            Total = total;
        }
    }

    public readonly struct LevelCompletedEvent : IGameEvent
    {
        public readonly LevelResult Result;
        public LevelCompletedEvent(LevelResult result) => Result = result;
    }

    /// <summary>Periodic timer/HUD update emitted by the level manager.</summary>
    public readonly struct TimerTickedEvent : IGameEvent
    {
        public readonly float ElapsedSeconds;
        public TimerTickedEvent(float elapsedSeconds) => ElapsedSeconds = elapsedSeconds;
    }

    public readonly struct InteractableToggledEvent : IGameEvent
    {
        public readonly string InteractableId;
        public readonly bool IsActive;
        public InteractableToggledEvent(string interactableId, bool isActive)
        {
            InteractableId = interactableId;
            IsActive = isActive;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Meta / economy / achievement events.
    // ---------------------------------------------------------------------------------------------

    public readonly struct FragmentsChangedEvent : IGameEvent
    {
        public readonly int Total;
        public readonly int Delta;
        public FragmentsChangedEvent(int total, int delta)
        {
            Total = total;
            Delta = delta;
        }
    }

    public readonly struct AchievementUnlockedEvent : IGameEvent
    {
        public readonly string AchievementId;
        public readonly string DisplayName;
        public AchievementUnlockedEvent(string achievementId, string displayName)
        {
            AchievementId = achievementId;
            DisplayName = displayName;
        }
    }

    public readonly struct GameSavedEvent : IGameEvent
    {
    }
}
