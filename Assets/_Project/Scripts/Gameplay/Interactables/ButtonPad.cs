using System.Collections.Generic;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// Momentary pressure pad: active only while at least one actor stands on its cell. The core
    /// of the echo-coordination puzzles — an echo can hold it down while the live player proceeds.
    /// </summary>
    public sealed class ButtonPad : MonoBehaviour, IActivator
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _idleColor = new(0.6f, 0.3f, 0.3f);
        [SerializeField] private Color _activeColor = new(0.3f, 1f, 0.5f);

        public string Id { get; private set; }
        public Vector2Int Cell { get; private set; }
        public bool IsActive { get; private set; }

        private IEventBus _eventBus;

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
            var nowActive = false;
            foreach (var a in actors)
            {
                if (a.IsAlive && a.Cell == Cell)
                {
                    nowActive = true;
                    break;
                }
            }

            if (nowActive != IsActive)
            {
                IsActive = nowActive;
                ApplyVisual();
                _eventBus?.Publish(new InteractableToggledEvent(Id, IsActive));
            }
        }

        public void ResetState()
        {
            IsActive = false;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_renderer != null)
            {
                _renderer.color = IsActive ? _activeColor : _idleColor;
            }
        }
    }
}
