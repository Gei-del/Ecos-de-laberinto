using System;
using UnityEngine;

namespace EcosDelLaberinto.Domain
{
    /// <summary>
    /// A single deterministic snapshot of an actor for one simulation tick. We record the
    /// absolute grid cell (not the raw input) so playback reproduces the run exactly regardless
    /// of how the rest of the world reacts — this is what guarantees "the echo repeats exactly
    /// the actions performed earlier".
    /// </summary>
    [Serializable]
    public struct EchoFrame
    {
        public int CellX;
        public int CellY;
        public byte Facing;        // cast of GridDirection
        public bool Interact;      // button / switch press this tick
        public bool AbilityUsed;   // character ability triggered this tick

        public EchoFrame(Vector2Int cell, GridDirection facing, bool interact, bool abilityUsed)
        {
            CellX = cell.x;
            CellY = cell.y;
            Facing = (byte)facing;
            Interact = interact;
            AbilityUsed = abilityUsed;
        }

        public Vector2Int Cell => new(CellX, CellY);
        public GridDirection FacingDirection => (GridDirection)Facing;
    }
}
