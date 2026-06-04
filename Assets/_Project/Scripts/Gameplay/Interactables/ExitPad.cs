using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// The level goal. Reached only by the live player. Optionally requires every crystal first
    /// (designer toggle), otherwise crystals only affect the star rating.
    /// </summary>
    public sealed class ExitPad : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _lockedColor = new(0.6f, 0.6f, 0.2f);
        [SerializeField] private Color _openColor = new(0.3f, 1f, 0.6f);

        public Vector2Int Cell { get; private set; }
        public bool RequireAllCrystals;

        public void BindRenderer(SpriteRenderer renderer) => _renderer = renderer;

        public void Initialize(Vector2Int cell, bool requireAllCrystals)
        {
            Cell = cell;
            RequireAllCrystals = requireAllCrystals;
            UpdateVisual(true);
        }

        public bool IsReached(IGridActor livePlayer, int crystalsRemaining)
        {
            var open = !RequireAllCrystals || crystalsRemaining <= 0;
            UpdateVisual(open);

            return open && livePlayer != null && livePlayer.IsAlive && livePlayer.Cell == Cell;
        }

        private void UpdateVisual(bool open)
        {
            if (_renderer != null)
            {
                _renderer.color = open ? _openColor : _lockedColor;
            }
        }
    }
}
