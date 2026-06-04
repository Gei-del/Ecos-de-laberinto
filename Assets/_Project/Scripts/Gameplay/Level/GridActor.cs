using EcosDelLaberinto.Domain;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>
    /// Shared base for the live player and echoes: holds the grid cell, smooth visual
    /// interpolation between cells, facing rotation and death handling. Subclasses implement how a
    /// cell is chosen each tick (live input vs recorded playback).
    /// </summary>
    public abstract class GridActor : MonoBehaviour, IGridActor
    {
        [SerializeField] protected SpriteRenderer Renderer;
        [SerializeField] private float _visualLerpSpeed = 14f;

        protected LevelGrid Grid;
        private Vector3 _targetPosition;

        public Vector2Int Cell { get; protected set; }
        public GridDirection Facing { get; protected set; } = GridDirection.Down;
        public bool IsAlive { get; protected set; } = true;
        public abstract bool IsLivePlayer { get; }
        public virtual bool InteractingThisTick { get; protected set; }
        public virtual bool CanPushHeavy => false;

        protected virtual void Update()
        {
            // Smooth visual motion independent of the fixed simulation tick.
            transform.position = Vector3.Lerp(transform.position, _targetPosition,
                1f - Mathf.Exp(-_visualLerpSpeed * UnityEngine.Time.deltaTime));
        }

        /// <summary>Assigns the sprite renderer when the actor is built programmatically.</summary>
        public void AssignRenderer(SpriteRenderer renderer)
        {
            Renderer = renderer;
        }

        public void SetCellImmediate(Vector2Int cell)
        {
            Cell = cell;
            _targetPosition = Grid != null ? Grid.CellToWorld(cell) : (Vector3)(Vector2)cell;
            transform.position = _targetPosition;
        }

        public void SetCellAnimated(Vector2Int cell)
        {
            Cell = cell;
            _targetPosition = Grid != null ? Grid.CellToWorld(cell) : (Vector3)(Vector2)cell;
        }

        public void ForceMoveTo(Vector2Int cell) => SetCellAnimated(cell);

        protected void SetFacing(GridDirection direction)
        {
            if (direction == GridDirection.None)
            {
                return;
            }

            Facing = direction;
            transform.rotation = Quaternion.Euler(0f, 0f, direction.ToZRotation());
        }

        public virtual void Kill(string cause)
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            OnKilled(cause);
        }

        protected virtual void OnKilled(string cause)
        {
        }

        protected void SetAlpha(float alpha)
        {
            if (Renderer != null)
            {
                var c = Renderer.color;
                c.a = alpha;
                Renderer.color = c;
            }
        }
    }
}
