using EcosDelLaberinto.Core.Events;
using UnityEngine;

namespace EcosDelLaberinto.Audio
{
    /// <summary>
    /// AudioSource-backed implementation. Subscribes to gameplay events to play the matching sfx,
    /// so adding a sound is just dropping a clip into the <see cref="AudioLibrary"/> under the
    /// expected key (e.g. "echo_spawn", "door_open", "crystal", "death").
    /// </summary>
    public sealed class AudioService : IAudioService
    {
        private readonly AudioSource _music;
        private readonly AudioSource _sfx;
        private readonly AudioLibrary _library;
        private readonly IEventBus _events;

        public AudioService(AudioSource music, AudioSource sfx, AudioLibrary library, IEventBus events)
        {
            _music = music;
            _sfx = sfx;
            _library = library;
            _events = events;

            if (_music != null)
            {
                _music.loop = true;
            }

            Subscribe();
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_music == null || clip == null)
            {
                return;
            }

            _music.clip = clip;
            _music.loop = loop;
            _music.Play();
        }

        public void StopMusic()
        {
            if (_music != null)
            {
                _music.Stop();
            }
        }

        public void PlaySfx(string key)
        {
            if (_sfx == null || _library == null)
            {
                return;
            }

            var clip = _library.GetSfx(key);
            if (clip != null)
            {
                _sfx.PlayOneShot(clip);
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (_music != null)
            {
                _music.volume = Mathf.Clamp01(volume);
            }
        }

        public void SetSfxVolume(float volume)
        {
            if (_sfx != null)
            {
                _sfx.volume = Mathf.Clamp01(volume);
            }
        }

        private void Subscribe()
        {
            _events.Subscribe<LoopStartedEvent>(_ => PlaySfx("echo_spawn"));
            _events.Subscribe<PlayerDiedEvent>(_ => PlaySfx("death"));
            _events.Subscribe<CrystalCollectedEvent>(_ => PlaySfx("crystal"));
            _events.Subscribe<InteractableToggledEvent>(e => PlaySfx(e.IsActive ? "door_open" : "door_close"));
            _events.Subscribe<LevelCompletedEvent>(_ => PlaySfx("win"));
        }
    }
}
