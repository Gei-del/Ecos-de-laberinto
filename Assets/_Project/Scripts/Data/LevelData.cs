using System.Collections.Generic;
using UnityEngine;

namespace EcosDelLaberinto.Data
{
    /// <summary>
    /// Data-driven level definition. The layout is authored as an ASCII grid so designers (and
    /// the procedural roadmap tools) can create levels without touching scenes. The
    /// <c>LevelBuilder</c> parses this into live GameObjects at runtime.
    ///
    /// Legend (one char per cell):
    ///   '#'  Wall
    ///   '.'  Floor (empty walkable)
    ///   ' '  Floor (treated as '.')
    ///   'P'  Player spawn
    ///   'X'  Exit / level goal
    ///   'C'  Crystal (collectible, 3 per level by design)
    ///   'B'  Button (momentary, active only while stood on)
    ///   'T'  Switch (toggle, stays on once pressed via interact)
    ///   'D'  Door (opens while its linked group is active)
    ///   'L'  Laser emitter (lethal beam, blocked by actors/blocks)
    ///   'O'  Pushable block
    ///   'F'  Fragment pickup (soft currency)
    ///   '~'  Temporal void (deadly pit, only crossable on a moving platform)
    ///   'M'  Moving platform home cell (travels along its line of '~' cells)
    /// Linking of buttons/doors is done by index order within the same level (see LevelBuilder).
    /// </summary>
    [CreateAssetMenu(menuName = "Ecos/Level", fileName = "Level_")]
    public sealed class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public string Id = "w1_l1";
        public int World = 1;
        public int IndexInWorld = 1;
        public string DisplayName = "Despertar";

        [Header("Layout")]
        [Tooltip("Each string is a row, top row first. All rows should share the same length.")]
        [TextArea(4, 20)]
        public string[] Rows =
        {
            "#########",
            "#P..B..D#",
            "#...#..C#",
            "#..L#..X#",
            "#########"
        };

        [Header("Star thresholds (override GameConfig defaults when > 0)")]
        public float ThreeStarTime = 0f;
        public float TwoStarTime = 0f;
        [Tooltip("Max deaths allowed to still earn 3 stars.")]
        public int ThreeStarMaxDeaths = 1;

        [Header("Tutorial")]
        [TextArea] public string TutorialHint;

        [Header("Rewards")]
        [Tooltip("Relic id granted the first time this level is completed (empty = none).")]
        public string GrantsRelicId = string.Empty;

        public int Height => Rows?.Length ?? 0;
        public int Width
        {
            get
            {
                var max = 0;
                if (Rows == null)
                {
                    return 0;
                }

                foreach (var r in Rows)
                {
                    if (r != null && r.Length > max)
                    {
                        max = r.Length;
                    }
                }

                return max;
            }
        }

        public char CellAt(int x, int y)
        {
            // y is measured from the bottom so it matches Unity's +Y up convention.
            var rowFromTop = Height - 1 - y;
            if (rowFromTop < 0 || rowFromTop >= Height)
            {
                return '#';
            }

            var row = Rows[rowFromTop];
            if (row == null || x < 0 || x >= row.Length)
            {
                return '#';
            }

            return row[x];
        }

        public int CountCrystals()
        {
            var count = 0;
            if (Rows == null)
            {
                return 0;
            }

            foreach (var row in Rows)
            {
                if (row == null)
                {
                    continue;
                }

                foreach (var c in row)
                {
                    if (c == 'C')
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public IEnumerable<Vector2Int> FindAll(char symbol)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (CellAt(x, y) == symbol)
                    {
                        yield return new Vector2Int(x, y);
                    }
                }
            }
        }
    }
}
