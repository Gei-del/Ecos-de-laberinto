using System;

namespace EcosDelLaberinto.Domain
{
    /// <summary>
    /// Outcome of a completed level. Pure data; the scoring rules live in the scoring service so
    /// they can be tuned without touching gameplay code.
    /// </summary>
    [Serializable]
    public struct LevelResult
    {
        public string LevelId;
        public float TimeSeconds;
        public int Deaths;
        public int EchoesUsed;
        public int CrystalsCollected;
        public int CrystalsTotal;
        public int Stars;            // 1..3
        public int FragmentsEarned;  // soft currency awarded

        public bool IsPerfect => Stars >= 3;
    }
}
