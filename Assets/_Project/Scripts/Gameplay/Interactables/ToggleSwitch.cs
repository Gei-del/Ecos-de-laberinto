using System.Collections.Generic;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// Latching switch: flips state when an actor presses "interact" while standing on it. Unlike a
    /// <see cref="ButtonPad"/> it stays in its new state after the actor leaves.
    /// </summary>
    public sealed class ToggleSwitch : MonoBehaviour, IActivator
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _offColor = new(0.5f, 0.5f, 0.2f);
        [SerializeField] private Color _onColor = new(0.2f, 0.9f, 1f);

        public string Id { get; private set; }
        public Vector2Int Cell { get; private set; }
        public bool IsActive { get; private set; }

        private IEventBus _eventBus;
        private bool _consumedThisPress;

        public void BindRenderer(SpriteRenderer renderer) => _renderer = renderer;

        public void Initialize(string id, Vector2Int cell, IEventBus eventBus)
        {
            Id = id;
            Cell = cell;
            _eventBus = eventBus;
            ApplyVisual();
        }

        public void Evaluate(IReadOnlyList<IGridActor> actors)
        {
            var pressedHere = false;
            foreach (var a in actors)
            {
                if (a.IsAlive && a.Cell == Cell && a.InteractingThisTick)
                {
                    pressedHere = true;
                    break;
                }
            }

            // Rising edge only, so holding interact doesn't oscillate every tick.
            if (pressedHere && !_consumedThisPress)
            {
                IsActive = !IsActive;
                ApplyVisual();
                _eventBus?.Publish(new InteractableToggledEvent(Id, IsActive));
            }

            _consumedThisPress = pressedHere;
        }

        public void ResetState()
        {
            IsActive = false;
            _consumedThisPress = false;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_renderer != null)
            {
                _renderer.color = IsActive ? _onColor : _offColor;
            }
        }
    }
}
