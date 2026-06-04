using EcosDelLaberinto.Data;
using EcosDelLaberinto.Domain;

namespace EcosDelLaberinto.Scoring
{
    public interface IScoreService
    {
        LevelResult Evaluate(LevelData level, float timeSeconds, int deaths, int echoesUsed,
            int crystalsCollected, int crystalsTotal);
    }
}
