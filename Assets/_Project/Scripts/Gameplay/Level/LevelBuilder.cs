using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Core.Utils;
using EcosDelLaberinto.Data;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Interactables;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>
    /// Parses a <see cref="LevelData"/> ASCII grid into a live, playable scene graph: floor/wall
    /// tiles, the <see cref="LevelGrid"/>, and every interactable. Fully data-driven — no per-level
    /// scenes or prefabs are required, which is what makes 65+ levels maintainable.
    /// </summary>
    public sealed class LevelBuilder
    {
        private static readonly Color FloorColor = new(0.12f, 0.13f, 0.20f);
        private static readonly Color WallColor = new(0.30f, 0.33f, 0.45f);
        private static readonly Color CrystalColor = new(0.4f, 0.9f, 1f);
        private static readonly Color BlockColor = new(0.7f, 0.55f, 0.3f);

        private readonly GameConfig _config;
        private readonly IEventBus _eventBus;

        public LevelBuilder(GameConfig config, IEventBus eventBus)
        {
            _config = config;
            _eventBus = eventBus;
        }

        public BuiltLevel Build(LevelData data, Transform parent)
        {
            var width = data.Width;
            var height = data.Height;
            var grid = new LevelGrid(width, height, _config.CellSize, Vector3.zero);

            var root = new GameObject($"Level_{data.Id}").transform;
            root.SetParent(parent, false);

            var built = new BuiltLevel { Grid = grid, Root = root };

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var symbol = data.CellAt(x, y);
                    var isWall = symbol == '#';

                    grid.SetWall(cell, isWall);
                    CreateTile(root, grid, cell, isWall ? WallColor : FloorColor, isWall ? 0 : -1);

                    if (isWall)
                    {
                        continue;
                    }

                    SpawnEntity(built, root, grid, data, cell, symbol);
                }
            }

            // AND-link every activator to every door: the signature "hold all buttons" puzzle.
            foreach (var door in built.Doors)
            {
                foreach (var activator in built.Activators)
                {
                    door.LinkActivator(activator);
                }
            }

            return built;
        }

        private void SpawnEntity(BuiltLevel built, Transform root, LevelGrid grid, LevelData data,
            Vector2Int cell, char symbol)
        {
            switch (symbol)
            {
                case 'P':
                    built.PlayerSpawn = cell;
                    break;
                case 'X':
                    built.Exit = CreateExit(root, grid, cell);
                    break;
                case 'C':
                    built.Crystals.Add(CreateCrystal(root, grid, data, cell, built.Crystals.Count));
                    break;
                case 'B':
                    built.Buttons.Add(CreateButton(root, grid, data, cell, built.Buttons.Count));
                    break;
                case 'T':
                    built.Switches.Add(CreateSwitch(root, grid, data, cell, built.Switches.Count));
                    break;
                case 'D':
                    built.Doors.Add(CreateDoor(root, grid, data, cell, built.Doors.Count));
                    break;
                case 'L':
                    built.Lasers.Add(CreateLaser(root, grid, data, cell, built.Lasers.Count));
                    break;
                case 'O':
                    built.Blocks.Add(CreateBlock(root, grid, cell));
                    break;
            }
        }

        private SpriteRenderer CreateTile(Transform root, LevelGrid grid, Vector2Int cell,
            Color color, int order)
        {
            var go = new GameObject($"Tile_{cell.x}_{cell.y}");
            go.transform.SetParent(root, false);
            go.transform.position = grid.CellToWorld(cell);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PrimitiveSprites.Square();
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        private ExitPad CreateExit(Transform root, LevelGrid grid, Vector2Int cell)
        {
            var sr = CreateTile(root, grid, cell, new Color(0.3f, 1f, 0.6f), 1);
            sr.sprite = PrimitiveSprites.Diamond();
            var exit = sr.gameObject.AddComponent<ExitPad>();
            sr.gameObject.name = "Exit";
            exit.BindRenderer(sr);
            exit.Initialize(cell, requireAllCrystals: false);
            return exit;
        }

        private Crystal CreateCrystal(Transform root, LevelGrid grid, LevelData data, Vector2Int cell,
            int index)
        {
            var sr = CreateTile(root, grid, cell, CrystalColor, 3);
            sr.sprite = PrimitiveSprites.Diamond();
            sr.gameObject.name = $"Crystal_{index}";
            var crystal = sr.gameObject.AddComponent<Crystal>();
            crystal.Initialize($"{data.Id}_c{index}", cell);
            return crystal;
        }

        private ButtonPad CreateButton(Transform root, LevelGrid grid, LevelData data, Vector2Int cell,
            int index)
        {
            var sr = CreateTile(root, grid, cell, new Color(0.6f, 0.3f, 0.3f), 1);
            sr.sprite = PrimitiveSprites.Circle();
            sr.gameObject.name = $"Button_{index}";
            var button = sr.gameObject.AddComponent<ButtonPad>();
            button.BindRenderer(sr);
            button.Initialize($"{data.Id}_b{index}", cell, _eventBus);
            return button;
        }

        private ToggleSwitch CreateSwitch(Transform root, LevelGrid grid, LevelData data,
            Vector2Int cell, int index)
        {
            var sr = CreateTile(root, grid, cell, new Color(0.5f, 0.5f, 0.2f), 1);
            sr.sprite = PrimitiveSprites.Circle();
            sr.gameObject.name = $"Switch_{index}";
            var sw = sr.gameObject.AddComponent<ToggleSwitch>();
            sw.BindRenderer(sr);
            sw.Initialize($"{data.Id}_t{index}", cell, _eventBus);
            return sw;
        }

        private Door CreateDoor(Transform root, LevelGrid grid, LevelData data, Vector2Int cell,
            int index)
        {
            var sr = CreateTile(root, grid, cell, new Color(0.8f, 0.2f, 0.2f), 2);
            sr.sprite = PrimitiveSprites.Square();
            sr.gameObject.name = $"Door_{index}";
            var door = sr.gameObject.AddComponent<Door>();
            door.BindRenderer(sr);
            door.Initialize($"{data.Id}_d{index}", cell, grid);
            return door;
        }

        private LaserEmitter CreateLaser(Transform root, LevelGrid grid, LevelData data,
            Vector2Int cell, int index)
        {
            var sr = CreateTile(root, grid, cell, new Color(1f, 0.3f, 0.3f), 2);
            sr.sprite = PrimitiveSprites.Square();
            sr.gameObject.name = $"Laser_{index}";

            var direction = ChooseLaserDirection(grid, data, cell);

            var lineGo = new GameObject("Beam");
            lineGo.transform.SetParent(sr.transform, false);
            var line = lineGo.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.sortingOrder = 4;
            line.numCapVertices = 2;

            var laser = sr.gameObject.AddComponent<LaserEmitter>();
            laser.BindLine(line);
            laser.Initialize($"{data.Id}_l{index}", cell, direction, grid);
            return laser;
        }

        private PushableBlock CreateBlock(Transform root, LevelGrid grid, Vector2Int cell)
        {
            var sr = CreateTile(root, grid, cell, BlockColor, 3);
            sr.sprite = PrimitiveSprites.Square();
            sr.gameObject.name = "Block";
            var block = sr.gameObject.AddComponent<PushableBlock>();
            block.Initialize(grid, cell);
            return block;
        }

        private static GridDirection ChooseLaserDirection(LevelGrid grid, LevelData data, Vector2Int cell)
        {
            // Fire toward the first open neighbour (the back of the emitter sits against a wall).
            Vector2Int[] order = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
            GridDirection[] dirs =
            {
                GridDirection.Right, GridDirection.Left, GridDirection.Up, GridDirection.Down
            };

            for (var i = 0; i < order.Length; i++)
            {
                if (!grid.IsWall(cell + order[i]))
                {
                    return dirs[i];
                }
            }

            return GridDirection.Right;
        }
    }
}
