using EcosDelLaberinto.Data;
using EcosDelLaberinto.Domain;
using UnityEngine;

namespace EcosDelLaberinto.Scoring
{
    /// <summary>
    /// Pure scoring rules. Stars are awarded from time + deaths + crystals; fragments are derived
    /// from stars and crystals. Kept isolated so the economy can be re-tuned without gameplay
    /// changes.
    /// </summary>
    public sealed class ScoreService : IScoreService
    {
        private readonly GameConfig _config;

        public ScoreService(GameConfig config)
        {
            _config = config;
        }

        public LevelResult Evaluate(LevelData level, float timeSeconds, int deaths, int echoesUsed,
            int crystalsCollected, int crystalsTotal)
        {
            var threeStarTime = level != null && level.ThreeStarTime > 0f
                ? level.ThreeStarTime
                : _config.DefaultThreeStarTime;
            var twoStarTime = level != null && level.TwoStarTime > 0f
                ? level.TwoStarTime
                : _config.DefaultTwoStarTime;
            var maxDeathsForThree = level != null ? level.ThreeStarMaxDeaths : 1;

            var stars = 1; // completing the level always grants at least one star.

            var allCrystals = crystalsTotal <= 0 || crystalsCollected >= crystalsTotal;

            if (timeSeconds <= twoStarTime)
            {
                stars = 2;
            }

            if (timeSeconds <= threeStarTime && deaths <= maxDeathsForThree && allCrystals)
            {
                stars = 3;
            }

            var fragments = stars * _config.FragmentsPerStar +
                            crystalsCollected * _config.FragmentsPerCrystal;

            return new LevelResult
            {
                LevelId = level != null ? level.Id : "unknown",
                TimeSeconds = timeSeconds,
                Deaths = deaths,
                EchoesUsed = echoesUsed,
                CrystalsCollected = crystalsCollected,
                CrystalsTotal = crystalsTotal,
                Stars = Mathf.Clamp(stars, 1, 3),
                FragmentsEarned = fragments
            };
        }
    }
}
