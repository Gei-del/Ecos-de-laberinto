using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcosDelLaberinto.Audio
{
    /// <summary>Data-driven mapping of sfx keys to clips, plus the looping music tracks.</summary>
    [CreateAssetMenu(menuName = "Ecos/Audio Library", fileName = "AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public struct SfxEntry
        {
            public string Key;
            public AudioClip Clip;
        }

        public AudioClip MenuMusic;
        public AudioClip GameplayMusic;
        public List<SfxEntry> Sfx = new();

        public AudioClip GetSfx(string key)
        {
            foreach (var e in Sfx)
            {
                if (e.Key == key)
                {
                    return e.Clip;
                }
            }

            return null;
        }
    }
}
