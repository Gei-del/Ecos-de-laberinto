using UnityEngine;

namespace EcosDelLaberinto.Core.Utils
{
    /// <summary>
    /// Thin logging facade so gameplay code never depends directly on <see cref="UnityEngine.Debug"/>.
    /// Logs are prefixed and can be globally muted for release builds.
    /// </summary>
    public static class GameLogger
    {
        private const string Prefix = "[Ecos]";

        public static bool Enabled = true;

        public static void Info(string message)
        {
            if (Enabled)
            {
                Debug.Log($"{Prefix} {message}");
            }
        }

        public static void Warn(string message)
        {
            if (Enabled)
            {
                Debug.LogWarning($"{Prefix} {message}");
            }
        }

        public static void Error(string message)
        {
            // Errors are always logged regardless of the Enabled flag.
            Debug.LogError($"{Prefix} {message}");
        }
    }
}
