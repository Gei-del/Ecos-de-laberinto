using System.Collections.Generic;
using UnityEngine;

namespace EcosDelLaberinto.Data
{
    /// <summary>
    /// Central registry of every data asset in the game. Injected into the container at boot so
    /// systems resolve content by id instead of holding scattered references.
    /// </summary>
    [CreateAssetMenu(menuName = "Ecos/Game Database", fileName = "GameDatabase")]
    public sealed class GameDatabase : ScriptableObject
    {
        public GameConfig Config;
        public List<CharacterData> Characters = new();
        public List<LevelData> Levels = new();
        public List<AchievementData> Achievements = new();

        private Dictionary<string, CharacterData> _characterById;
        private Dictionary<string, LevelData> _levelById;
        private Dictionary<string, AchievementData> _achievementById;

        public void BuildIndices()
        {
            _characterById = new Dictionary<string, CharacterData>();
            foreach (var c in Characters)
            {
                if (c != null && !string.IsNullOrEmpty(c.Id))
                {
                    _characterById[c.Id] = c;
                }
            }

            _levelById = new Dictionary<string, LevelData>();
            foreach (var l in Levels)
            {
                if (l != null && !string.IsNullOrEmpty(l.Id))
                {
                    _levelById[l.Id] = l;
                }
            }

            _achievementById = new Dictionary<string, AchievementData>();
            foreach (var a in Achievements)
            {
                if (a != null && !string.IsNullOrEmpty(a.Id))
                {
                    _achievementById[a.Id] = a;
                }
            }
        }

        public CharacterData GetCharacter(string id)
        {
            EnsureIndices();
            return _characterById.TryGetValue(id, out var c) ? c : null;
        }

        public LevelData GetLevel(string id)
        {
            EnsureIndices();
            return _levelById.TryGetValue(id, out var l) ? l : null;
        }

        public AchievementData GetAchievement(string id)
        {
            EnsureIndices();
            return _achievementById.TryGetValue(id, out var a) ? a : null;
        }

        public LevelData GetLevelByOrder(int zeroBasedIndex)
        {
            if (Levels == null || zeroBasedIndex < 0 || zeroBasedIndex >= Levels.Count)
            {
                return null;
            }

            return Levels[zeroBasedIndex];
        }

        public int IndexOfLevel(string id)
        {
            for (var i = 0; i < Levels.Count; i++)
            {
                if (Levels[i] != null && Levels[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private void EnsureIndices()
        {
            if (_levelById == null)
            {
                BuildIndices();
            }
        }
    }
}
