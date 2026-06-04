using UnityEngine;

namespace EcosDelLaberinto.Data
{
    /// <summary>Data-driven achievement definition. Maps 1:1 to a Steam achievement API name.</summary>
    [CreateAssetMenu(menuName = "Ecos/Achievement", fileName = "Achievement_")]
    public sealed class AchievementData : ScriptableObject
    {
        public string Id = "first_echo";
        public string DisplayName = "Primer Eco";
        [TextArea] public string Description = "Crea tu primer eco temporal.";
        public bool Hidden;
        public Sprite Icon;
        [Tooltip("Optional Steam API name; defaults to Id if empty.")]
        public string SteamApiName;
    }
}
