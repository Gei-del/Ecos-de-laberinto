using System.Collections.Generic;
using EcosDelLaberinto.Gameplay.Interactables;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>Aggregates every runtime object produced from a <c>LevelData</c> asset.</summary>
    public sealed class BuiltLevel
    {
        public LevelGrid Grid;
        public Transform Root;
        public Vector2Int PlayerSpawn;
        public ExitPad Exit;

        public readonly List<ButtonPad> Buttons = new();
        public readonly List<ToggleSwitch> Switches = new();
        public readonly List<Door> Doors = new();
        public readonly List<LaserEmitter> Lasers = new();
        public readonly List<Crystal> Crystals = new();
        public readonly List<PushableBlock> Blocks = new();

        public int CrystalsTotal => Crystals.Count;

        public IEnumerable<IActivator> Activators
        {
            get
            {
                foreach (var b in Buttons) yield return b;
                foreach (var s in Switches) yield return s;
            }
        }
    }
}
