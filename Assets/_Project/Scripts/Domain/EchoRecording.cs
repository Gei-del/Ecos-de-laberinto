using System;
using System.Collections.Generic;

namespace EcosDelLaberinto.Domain
{
    /// <summary>
    /// An immutable-once-sealed timeline of <see cref="EchoFrame"/>s produced by a single loop.
    /// When the player dies or restarts, the active recording is sealed and turned into an echo
    /// that is replayed in every subsequent loop.
    /// </summary>
    [Serializable]
    public sealed class EchoRecording
    {
        public string CharacterId;
        public List<EchoFrame> Frames = new();

        public int Length => Frames.Count;

        public EchoRecording()
        {
        }

        public EchoRecording(string characterId)
        {
            CharacterId = characterId;
        }

        public void Append(in EchoFrame frame)
        {
            Frames.Add(frame);
        }

        /// <summary>Returns the frame for a given tick, clamping to the last frame so a finished
        /// echo simply "freezes" in place instead of throwing.</summary>
        public EchoFrame FrameAt(int tick)
        {
            if (Frames.Count == 0)
            {
                return default;
            }

            if (tick < 0)
            {
                tick = 0;
            }

            if (tick >= Frames.Count)
            {
                tick = Frames.Count - 1;
            }

            return Frames[tick];
        }

        public bool HasFinished(int tick) => tick >= Frames.Count;
    }
}
