using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// A block that actors can push one cell at a time. Heavy blocks can only be moved by a
    /// character whose ability grants <see cref="IGridActor.CanPushHeavy"/> (Atlas).
    /// </summary>
    public sealed class PushableBlock : MonoBehaviour
    {
        public bool IsHeavy;

        public Vector2Int Cell { get; private set; }

        private LevelGrid _grid;
        private Vector2Int _spawnCell;

        public void Initialize(LevelGrid grid, Vector2Int cell)
        {
            _grid = grid;
            _spawnCell = cell;
            SetCell(cell);
        }

        public void ResetToSpawn() => SetCell(_spawnCell);

        public void SetCell(Vector2Int cell)
        {
            if (_grid != null)
            {
                _grid.UnblockCell(Cell);
            }

            Cell = cell;

            if (_grid != null)
            {
                _grid.BlockCell(cell);
                transform.position = _grid.CellToWorld(cell);
            }
        }
    }
}
