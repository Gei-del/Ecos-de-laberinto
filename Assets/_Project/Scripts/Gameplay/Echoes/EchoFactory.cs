using EcosDelLaberinto.Core.Utils;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Echoes
{
    /// <summary>
    /// Factory pattern: builds <see cref="EchoActor"/> GameObjects from a sealed recording. Echoes
    /// are created programmatically with a placeholder sprite, parented under a container so a level
    /// reset can dispose them all at once.
    /// </summary>
    public sealed class EchoFactory
    {
        private readonly Transform _container;
        private readonly LevelGrid _grid;

        public EchoFactory(Transform container, LevelGrid grid)
        {
            _container = container;
            _grid = grid;
        }

        public EchoActor Create(EchoRecording recording, int lifetimeTicks, int index)
        {
            var go = new GameObject($"Echo_{index}");
            go.transform.SetParent(_container, false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = PrimitiveSprites.Circle();
            renderer.sortingOrder = 5;

            var echo = go.AddComponent<EchoActor>();
            // Wire the serialized renderer field through reflection-free initialization.
            echo.AssignRenderer(renderer);
            echo.Configure(_grid, recording, lifetimeTicks);
            return echo;
        }
    }
}
