using EcosDelLaberinto.Domain;
using UnityEngine;

namespace EcosDelLaberinto.Input
{
    /// <summary>
    /// Reads the legacy Input Manager axes/keys (WASD + arrows + gamepad d-pad/stick). Polled once
    /// per frame from the bootstrap; "pressed" flags are latched so they survive until the next
    /// simulation tick consumes them.
    /// </summary>
    public sealed class UnityInputService : IInputService
    {
        public GridDirection MoveDirection { get; private set; }
        public bool InteractHeld { get; private set; }
        public bool AbilityPressed { get; private set; }
        public bool RestartPressed { get; private set; }
        public bool PausePressed { get; private set; }

        public void Poll()
        {
            var x = UnityEngine.Input.GetAxisRaw("Horizontal");
            var y = UnityEngine.Input.GetAxisRaw("Vertical");
            MoveDirection = GridDirectionExtensions.FromInput(x, y);

            InteractHeld = UnityEngine.Input.GetKey(KeyCode.Space) ||
                           UnityEngine.Input.GetKey(KeyCode.E) ||
                           UnityEngine.Input.GetButton("Fire1");

            // Latch one-shot actions; the consumer clears them by reading once per tick.
            AbilityPressed |= UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) ||
                              UnityEngine.Input.GetKeyDown(KeyCode.Q) ||
                              UnityEngine.Input.GetButtonDown("Fire2");

            RestartPressed |= UnityEngine.Input.GetKeyDown(KeyCode.R);
            PausePressed |= UnityEngine.Input.GetKeyDown(KeyCode.Escape) ||
                            UnityEngine.Input.GetKeyDown(KeyCode.P);
        }

        public void ConsumeOneShots()
        {
            AbilityPressed = false;
            RestartPressed = false;
            PausePressed = false;
        }
    }
}
