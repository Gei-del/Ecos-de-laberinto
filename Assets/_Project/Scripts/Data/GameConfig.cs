using UnityEngine;

namespace EcosDelLaberinto.Data
{
    /// <summary>
    /// Global tunables. A single asset injected through the container so balancing never requires
    /// recompiling code.
    /// </summary>
    [CreateAssetMenu(menuName = "Ecos/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Simulation")]
        [Tooltip("Fixed simulation ticks per second. Echo recording/playback are driven by this " +
                 "to stay fully deterministic.")]
        public int TicksPerSecond = 10;

        [Tooltip("World size of one grid cell in Unity units.")]
        public float CellSize = 1f;

        [Header("Echoes")]
        [Tooltip("Maximum number of simultaneous echoes (design cap).")]
        public int MaxEchoes = 10;

        [Tooltip("Base lifetime of an echo in seconds before the character multiplier.")]
        public float BaseEchoLifetimeSeconds = 30f;

        [Header("Moving platforms")]
        [Tooltip("Ticks a moving platform spends on each cell before advancing to the next.")]
        public int PlatformStepTicks = 4;

        [Header("Scoring")]
        public int FragmentsPerStar = 25;
        public int FragmentsPerCrystal = 10;

        [Tooltip("Time (s) under which the level still earns the 3rd star.")]
        public float DefaultThreeStarTime = 30f;
        [Tooltip("Time (s) under which the level still earns the 2nd star.")]
        public float DefaultTwoStarTime = 60f;

        public int TicksToSeconds(int ticks) => Mathf.RoundToInt(ticks / (float)TicksPerSecond);
        public float TickInterval => 1f / Mathf.Max(1, TicksPerSecond);
        public int EchoLifetimeTicks(float multiplier) =>
            Mathf.RoundToInt(BaseEchoLifetimeSeconds * Mathf.Max(0.01f, multiplier) * TicksPerSecond);
    }
}
