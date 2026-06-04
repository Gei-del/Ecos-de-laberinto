using System;
using System.Collections.Generic;

namespace EcosDelLaberinto.Persistence
{
    /// <summary>Per-level persisted progress.</summary>
    [Serializable]
    public sealed class LevelProgress
    {
        public string LevelId;
        public int BestStars;
        public float BestTimeSeconds = -1f;
        public int CrystalsCollected;
        public bool Completed;
    }

    [Serializable]
    public sealed class SettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 1f;
        public bool FullScreen = true;
        public string Language = "es";
    }

    /// <summary>
    /// Root serialisable save document. Plain fields and List/Dictionary-friendly structures so it
    /// round-trips cleanly through <see cref="UnityEngine.JsonUtility"/>.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public int SaveVersion = 1;
        public string SelectedCharacterId = "nova";
        public int TemporalFragments;

        public List<string> UnlockedCharacters = new() { "nova" };
        public List<string> UnlockedRelics = new();
        public List<string> UnlockedAchievements = new();
        public List<string> PurchasedCosmetics = new();
        public List<LevelProgress> Levels = new();

        public SettingsData Settings = new();

        public LevelProgress GetOrCreateLevel(string levelId)
        {
            foreach (var lp in Levels)
            {
                if (lp.LevelId == levelId)
                {
                    return lp;
                }
            }

            var created = new LevelProgress { LevelId = levelId };
            Levels.Add(created);
            return created;
        }

        public bool IsCharacterUnlocked(string id) => UnlockedCharacters.Contains(id);
        public bool IsAchievementUnlocked(string id) => UnlockedAchievements.Contains(id);
    }
}
