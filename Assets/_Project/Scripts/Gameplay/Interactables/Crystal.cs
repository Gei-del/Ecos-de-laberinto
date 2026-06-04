using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>
    /// Collectible Temporal Crystal (3 per level). Only the live player collects it; echoes pass
    /// through. Collection persists across loops within the same level attempt.
    /// </summary>
    public sealed class Crystal : MonoBehaviour
    {
        public string Id { get; private set; }
        public Vector2Int Cell { get; private set; }
        public bool Collected { get; private set; }

        public void Initialize(string id, Vector2Int cell)
        {
            Id = id;
            Cell = cell;
            Collected = false;
            gameObject.SetActive(true);
        }

        /// <summary>Returns true the tick it is collected.</summary>
        public bool TryCollect(IGridActor livePlayer)
        {
            if (Collected || livePlayer == null || !livePlayer.IsAlive)
            {
                return false;
            }

            if (livePlayer.Cell == Cell)
            {
                Collected = true;
                gameObject.SetActive(false);
                return true;
            }

            return false;
        }
    }
}
