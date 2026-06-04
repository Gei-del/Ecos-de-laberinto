using UnityEngine;

namespace EcosDelLaberinto.Audio
{
    /// <summary>
    /// Audio facade. Clips are optional so the game runs silently until a sound designer wires the
    /// AudioClips on the <see cref="AudioLibrary"/> asset — no code changes required.
    /// </summary>
    public interface IAudioService
    {
        void PlayMusic(AudioClip clip, bool loop = true);
        void StopMusic();
        void PlaySfx(string key);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
    }
}
