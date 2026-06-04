using System.Collections.Generic;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// Fires a lethal beam in a fixed direction. The beam is stopped by walls, closed doors,
    /// pushable blocks AND by any actor standing in its path. The live player dies if hit; echoes
    /// instead block the beam and survive — this is the intended "use an echo to shield the laser"
    /// puzzle mechanic. Recomputed every tick because the world changes.
    /// </summary>
    public sealed class LaserEmitter : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField] private Color _beamColor = new(1f, 0.25f, 0.25f, 0.9f);
        [SerializeField] private bool _enabledBeam = true;

        public string Id { get; private set; }
        public Vector2Int Cell { get; private set; }
        public GridDirection Direction { get; private set; }

        private LevelGrid _grid;
        private readonly List<Vector2Int> _beam = new();

        public IReadOnlyList<Vector2Int> BeamCells => _beam;

        public void BindLine(LineRenderer line) => _line = line;

        public void Initialize(string id, Vector2Int cell, GridDirection direction, LevelGrid grid)
        {
            Id = id;
            Cell = cell;
            Direction = direction;
            _grid = grid;

            if (_line != null)
            {
                _line.startColor = _beamColor;
                _line.endColor = _beamColor;
                _line.widthMultiplier = 0.15f;
            }
        }

        /// <summary>Recompute the beam and kill the live player if caught. Returns true if it killed.</summary>
        public bool Evaluate(IReadOnlyList<IGridActor> actors)
        {
            _beam.Clear();
            if (!_enabledBeam || Direction == GridDirection.None)
            {
                if (_line != null) _line.enabled = false;
                return false;
            }

            var offset = Direction.ToOffset();
            var probe = Cell + offset;
            var guard = 0;
            var killedPlayer = false;

            while (_grid.InBounds(probe) && !_grid.IsWall(probe) &&
                   !_grid.IsDynamicallyBlocked(probe) && guard < 512)
            {
                _beam.Add(probe);

                // An actor in the cell stops the beam. Echoes shield; the live player dies.
                var blocker = FindActorAt(actors, probe);
                if (blocker != null)
                {
                    if (blocker.IsLivePlayer)
                    {
                        blocker.Kill("laser");
                        killedPlayer = true;
                    }

                    break;
                }

                probe += offset;
                guard++;
            }

            DrawBeam();
            return killedPlayer;
        }

        public void ResetState()
        {
            _beam.Clear();
            if (_line != null)
            {
                _line.enabled = false;
            }
        }

        private static IGridActor FindActorAt(IReadOnlyList<IGridActor> actors, Vector2Int cell)
        {
            foreach (var a in actors)
            {
                if (a.IsAlive && a.Cell == cell)
                {
                    return a;
                }
            }

            return null;
        }

        private void DrawBeam()
        {
            if (_line == null)
            {
                return;
            }

            _line.enabled = _beam.Count > 0;
            _line.positionCount = 2;
            _line.SetPosition(0, _grid.CellToWorld(Cell));
            var end = _beam.Count > 0 ? _beam[_beam.Count - 1] : Cell;
            _line.SetPosition(1, _grid.CellToWorld(end));
        }
    }
}
