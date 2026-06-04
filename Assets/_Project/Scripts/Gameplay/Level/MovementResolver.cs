using System.Collections.Generic;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Interactables;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>
    /// Single source of truth for "can this actor step there?". Shared by the live player and by
    /// echoes so block pushing is reproduced identically during replay. Walls and closed doors
    /// block; pushable blocks slide one cell if the space beyond is free.
    /// </summary>
    public sealed class MovementResolver
    {
        private readonly LevelGrid _grid;
        private readonly Dictionary<Vector2Int, PushableBlock> _blocks = new();

        public MovementResolver(LevelGrid grid)
        {
            _grid = grid;
        }

        public void RegisterBlock(PushableBlock block)
        {
            _blocks[block.Cell] = block;
        }

        public void RebuildBlockIndex(IEnumerable<PushableBlock> blocks)
        {
            _blocks.Clear();
            foreach (var b in blocks)
            {
                _blocks[b.Cell] = b;
            }
        }

        /// <summary>
        /// Attempts to move <paramref name="actor"/> one cell in <paramref name="direction"/>.
        /// Returns the resulting cell (unchanged if blocked).
        /// </summary>
        public Vector2Int Resolve(IGridActor actor, GridDirection direction)
        {
            if (direction == GridDirection.None)
            {
                return actor.Cell;
            }

            var offset = direction.ToOffset();
            var target = actor.Cell + offset;

            if (_grid.IsWall(target))
            {
                return actor.Cell;
            }

            // Pushable block in the way?
            if (_blocks.TryGetValue(target, out var block))
            {
                if (block.IsHeavy && !actor.CanPushHeavy)
                {
                    return actor.Cell;
                }

                var beyond = target + offset;
                if (_grid.IsWall(beyond) || _blocks.ContainsKey(beyond))
                {
                    return actor.Cell; // no room to push
                }

                // Slide the block, keep the index in sync.
                _blocks.Remove(target);
                block.SetCell(beyond);
                _blocks[beyond] = block;
                return target;
            }

            // Closed doors register as dynamic blockers.
            if (_grid.IsDynamicallyBlocked(target))
            {
                return actor.Cell;
            }

            return target;
        }
    }
}
