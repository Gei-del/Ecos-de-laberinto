using System.Collections.Generic;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// A door that blocks its cell while closed. It opens when its linked activators satisfy the
    /// <see cref="RequireAll"/> rule (AND by default — the signature "hold every button with echoes"
    /// puzzle). Open/closed state is mirrored into the <see cref="LevelGrid"/> so movement and
    /// lasers respect it.
    /// </summary>
    public sealed class Door : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _closedColor = new(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _openColor = new(0.2f, 0.8f, 0.3f);

        public string Id { get; private set; }
        public Vector2Int Cell { get; private set; }
        public bool RequireAll = true;
        public bool IsOpen { get; private set; }

        private LevelGrid _grid;
        private readonly List<IActivator> _activators = new();

        public void BindRenderer(SpriteRenderer renderer) => _renderer = renderer;

        public void Initialize(string id, Vector2Int cell, LevelGrid grid)
        {
            Id = id;
            Cell = cell;
            _grid = grid;
            SetOpen(false);
        }

        public void LinkActivator(IActivator activator)
        {
            if (activator != null && !_activators.Contains(activator))
            {
                _activators.Add(activator);
            }
        }

        public void Evaluate()
        {
            // A door with no linked activators stays closed (acts as a static gate).
            var shouldOpen = _activators.Count > 0;
            if (RequireAll)
            {
                foreach (var a in _activators)
                {
                    if (!a.IsActive)
                    {
                        shouldOpen = false;
                        break;
                    }
                }
            }
            else
            {
                shouldOpen = false;
                foreach (var a in _activators)
                {
                    if (a.IsActive)
                    {
                        shouldOpen = true;
                        break;
                    }
                }
            }

            if (shouldOpen != IsOpen)
            {
                SetOpen(shouldOpen);
            }
        }

        public void ResetState() => SetOpen(false);

        private void SetOpen(bool open)
        {
            IsOpen = open;
            if (_grid != null)
            {
                if (open)
                {
                    _grid.UnblockCell(Cell);
                }
                else
                {
                    _grid.BlockCell(Cell);
                }
            }

            if (_renderer != null)
            {
                _renderer.color = open ? _openColor : _closedColor;
                _renderer.enabled = !open; // hide the door panel when open
            }
        }
    }
}
