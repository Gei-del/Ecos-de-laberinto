using System.Collections.Generic;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>
    /// Runtime spatial model of the current level. Holds static walls plus a dynamic blocker set
    /// (closed doors, pushable blocks). Movement and lasers query this instead of using physics
    /// raycasts, keeping the simulation deterministic.
    /// </summary>
    public sealed class LevelGrid
    {
        private readonly bool[,] _walls;
        private readonly HashSet<Vector2Int> _dynamicBlockers = new();
        private readonly HashSet<Vector2Int> _voids = new();
        private readonly float _cellSize;
        private readonly Vector3 _origin;

        public int Width { get; }
        public int Height { get; }

        public LevelGrid(int width, int height, float cellSize, Vector3 origin)
        {
            Width = width;
            Height = height;
            _cellSize = cellSize;
            _origin = origin;
            _walls = new bool[width, height];
        }

        public void SetWall(Vector2Int cell, bool isWall)
        {
            if (InBounds(cell))
            {
                _walls[cell.x, cell.y] = isWall;
            }
        }

        public bool IsWall(Vector2Int cell) => !InBounds(cell) || _walls[cell.x, cell.y];

        public bool InBounds(Vector2Int cell) =>
            cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;

        public void BlockCell(Vector2Int cell) => _dynamicBlockers.Add(cell);
        public void UnblockCell(Vector2Int cell) => _dynamicBlockers.Remove(cell);

        public void MoveBlocker(Vector2Int from, Vector2Int to)
        {
            _dynamicBlockers.Remove(from);
            _dynamicBlockers.Add(to);
        }

        public bool IsDynamicallyBlocked(Vector2Int cell) => _dynamicBlockers.Contains(cell);

        /// <summary>
        /// Marks a cell as a temporal void (a pit). Voids are non-walkable by default — only a
        /// moving platform sitting on the cell makes it passable — so they are registered as
        /// dynamic blockers and platforms unblock/re-block them as they travel.
        /// </summary>
        public void SetVoid(Vector2Int cell)
        {
            if (InBounds(cell))
            {
                _voids.Add(cell);
                _dynamicBlockers.Add(cell);
            }
        }

        public bool IsVoid(Vector2Int cell) => _voids.Contains(cell);

        /// <summary>Walkable = inside bounds, not a wall and not currently blocked.</summary>
        public bool IsWalkable(Vector2Int cell) =>
            InBounds(cell) && !_walls[cell.x, cell.y] && !_dynamicBlockers.Contains(cell);

        public Vector3 CellToWorld(Vector2Int cell) =>
            _origin + new Vector3(cell.x * _cellSize, cell.y * _cellSize, 0f);

        public Vector2Int WorldToCell(Vector3 world)
        {
            var local = world - _origin;
            return new Vector2Int(
                Mathf.RoundToInt(local.x / _cellSize),
                Mathf.RoundToInt(local.y / _cellSize));
        }

        public Vector3 Center => _origin +
            new Vector3((Width - 1) * 0.5f * _cellSize, (Height - 1) * 0.5f * _cellSize, 0f);
    }
}
