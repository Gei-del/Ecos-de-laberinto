using EcosDelLaberinto.Domain;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>
    /// Anything that occupies a grid cell and can be stepped/killed: the live player and every
    /// echo. Mechanisms treat the player and echoes uniformly through this interface.
    /// </summary>
    public interface IGridActor
    {
        Vector2Int Cell { get; }
        GridDirection Facing { get; }
        bool IsAlive { get; }
        bool IsLivePlayer { get; }
        bool InteractingThisTick { get; }
        bool CanPushHeavy { get; }

        void Kill(string cause);
        void ForceMoveTo(Vector2Int cell);
    }
}
