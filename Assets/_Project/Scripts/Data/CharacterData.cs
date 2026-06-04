using UnityEngine;

namespace EcosDelLaberinto.Data
{
    /// <summary>
    /// Data-driven definition of a playable character. Designers create one asset per character
    /// (Nova, Atlas, Echo, Chrona) under Assets/_Project/ScriptableObjects/Characters.
    /// </summary>
    [CreateAssetMenu(menuName = "Ecos/Character", fileName = "Character_")]
    public sealed class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable id used in save files. Never rename once shipped.")]
        public string Id = "nova";
        public string DisplayName = "Nova";
        [TextArea] public string Description;

        [Header("Ability")]
        public CharacterAbility Ability = CharacterAbility.LongerEchoes;
        [Tooltip("Multiplier applied to the base echo lifetime (Nova > 1).")]
        public float EchoLifetimeMultiplier = 1f;
        [Tooltip("Cooldown in seconds for active abilities (Echo swap, Chrona rewind).")]
        public float AbilityCooldown = 5f;

        [Header("Progression")]
        public bool UnlockedByDefault = true;
        [Tooltip("Cost in Temporal Fragments to unlock in the store.")]
        public int UnlockCost = 0;

        [Header("Presentation")]
        public Sprite Portrait;
        public Sprite GameplaySprite;
        public Color TintColor = Color.white;
    }
}
