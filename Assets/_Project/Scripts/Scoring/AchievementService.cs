using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Data;
using EcosDelLaberinto.Persistence;

namespace EcosDelLaberinto.Scoring
{
    /// <summary>
    /// Listens to gameplay events and unlocks achievements. Definitions live as data assets; this
    /// service wires the runtime conditions for the milestone set. Unlocks are persisted and
    /// re-published as <see cref="AchievementUnlockedEvent"/> for the toast UI and Steam layer.
    /// </summary>
    public sealed class AchievementService
    {
        private readonly IEventBus _events;
        private readonly ISaveService _save;
        private readonly GameDatabase _db;

        public AchievementService(IEventBus events, ISaveService save, GameDatabase db)
        {
            _events = events;
            _save = save;
            _db = db;

            _events.Subscribe<LoopStartedEvent>(OnLoopStarted);
            _events.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _events.Subscribe<FragmentsChangedEvent>(OnFragmentsChanged);
        }

        private void OnLoopStarted(LoopStartedEvent e)
        {
            if (e.ActiveEchoes >= 1)
            {
                Unlock("first_echo");
            }

            if (e.ActiveEchoes >= 5)
            {
                Unlock("echo_master");
            }
        }

        private void OnLevelCompleted(LevelCompletedEvent e)
        {
            var r = e.Result;

            if (r.Deaths == 0)
            {
                Unlock("no_death");
            }

            if (r.IsPerfect)
            {
                Unlock("perfectionist");
            }

            if (r.CrystalsTotal > 0 && r.CrystalsCollected >= r.CrystalsTotal)
            {
                Unlock("collector");
            }

            // Count completed levels for the "Temporal Master" milestone.
            var completed = 0;
            foreach (var lp in _save.Data.Levels)
            {
                if (lp.Completed)
                {
                    completed++;
                }
            }

            if (completed >= 10)
            {
                Unlock("temporal_master");
            }
        }

        private void OnFragmentsChanged(FragmentsChangedEvent e)
        {
            if (e.Total >= 1000)
            {
                Unlock("time_dominator");
            }
        }

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || _save.Data.IsAchievementUnlocked(id))
            {
                return;
            }

            _save.Data.UnlockedAchievements.Add(id);
            _save.Save();

            var def = _db != null ? _db.GetAchievement(id) : null;
            var name = def != null ? def.DisplayName : id;
            _events.Publish(new AchievementUnlockedEvent(id, name));
        }
    }
}
