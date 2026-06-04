using System.Collections.Generic;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// A platform that ferries actors across temporal voids. It travels back and forth along a fixed
    /// straight path of void cells, advancing one cell every <c>stepTicks</c> ticks. Its position is
    /// a pure function of the tick index (reset at the start of every loop), so it stays perfectly
    /// deterministic — which means echoes that rode it in a previous loop replay onto it exactly.
    ///
    /// While the platform occupies a void cell that cell becomes passable; when it leaves, the cell
    /// reverts to a deadly pit. The live player standing on the platform is carried with it.
    /// </summary>
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        public string Id { get; private set; }
        public Vector2Int Cell { get; private set; }

        private readonly List<Vector2Int> _path = new();
        private LevelGrid _grid;
        private int _stepTicks = 4;

        public void BindRenderer(SpriteRenderer renderer) => _renderer = renderer;

        public void Initialize(string id, IReadOnlyList<Vector2Int> path, int stepTicks, LevelGrid grid)
        {
            Id = id;
            _grid = grid;
            _stepTicks = Mathf.Max(1, stepTicks);
            _path.Clear();
            if (path != null)
            {
                _path.AddRange(path);
            }

            if (_path.Count == 0)
            {
                _path.Add(Cell);
            }

            ResetState();
        }

        /// <summary>Cell the platform occupies at the given tick (ping-pong along the path).</summary>
        public Vector2Int CellForTick(int tick)
        {
            var n = _path.Count;
            if (n <= 1)
            {
                return _path[0];
            }

            var cycle = 2 * (n - 1);
            var phase = (tick / _stepTicks) % cycle;
            var index = phase < n ? phase : cycle - phase;
            return _path[index];
        }

        /// <summary>Advance to the cell for this tick, carrying the live player if it rides along.</summary>
        public void ApplyTick(int tick, IGridActor livePlayer)
        {
            var target = CellForTick(tick);
            if (target == Cell)
            {
                return;
            }

            if (livePlayer != null && livePlayer.IsAlive && livePlayer.IsLivePlayer &&
                livePlayer.Cell == Cell)
            {
                livePlayer.ForceMoveTo(target);
            }

            _grid?.BlockCell(Cell);
            _grid?.UnblockCell(target);
            Cell = target;
            ApplyVisualPosition();
        }

        public void ResetState()
        {
            // Re-seal whatever cell we were on, then settle back onto the home cell.
            _grid?.BlockCell(Cell);
            Cell = _path[0];
            _grid?.UnblockCell(Cell);
            ApplyVisualPosition();
        }

        private void ApplyVisualPosition()
        {
            if (_grid != null)
            {
                transform.position = _grid.CellToWorld(Cell);
            }
        }
    }
}
