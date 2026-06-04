using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Echoes
{
    /// <summary>
    /// A temporal echo: a translucent replay of a previous loop. It reproduces the recorded cells
    /// exactly (deterministic), keeps holding its last position once the recording ends (so it can
    /// keep a button pressed), and fades out when its lifetime expires.
    /// </summary>
    public sealed class EchoActor : GridActor
    {
        [SerializeField] private Color _echoTint = new(0.4f, 0.8f, 1f, 0.6f);

        public override bool IsLivePlayer => false;

        private EchoRecording _recording;
        private int _lifetimeTicks;

        public int LifetimeTicks => _lifetimeTicks;

        public void Configure(LevelGrid grid, EchoRecording recording, int lifetimeTicks)
        {
            Grid = grid;
            _recording = recording;
            _lifetimeTicks = lifetimeTicks;
        }

        public void BeginLoop()
        {
            IsAlive = true;
            if (Renderer != null)
            {
                Renderer.color = _echoTint;
            }

            var first = _recording != null && _recording.Length > 0
                ? _recording.FrameAt(0)
                : default;
            SetCellImmediate(first.Cell);
            SetFacing(first.FacingDirection);
            gameObject.SetActive(true);
        }

        public void Step(int tick)
        {
            if (!IsAlive || _recording == null)
            {
                return;
            }

            if (tick >= _lifetimeTicks)
            {
                Expire();
                return;
            }

            var frame = _recording.FrameAt(tick);
            SetCellAnimated(frame.Cell);
            SetFacing(frame.FacingDirection);
            InteractingThisTick = frame.Interact;
        }

        private void Expire()
        {
            IsAlive = false;
            InteractingThisTick = false;
            gameObject.SetActive(false);
        }
    }
}
