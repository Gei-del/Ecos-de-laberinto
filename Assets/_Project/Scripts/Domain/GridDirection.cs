using UnityEngine;

namespace EcosDelLaberinto.Domain
{
    /// <summary>
    /// The four cardinal directions used by the top-down grid movement, plus a neutral value.
    /// </summary>
    public enum GridDirection : byte
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4
    }

    public static class GridDirectionExtensions
    {
        public static Vector2Int ToOffset(this GridDirection direction)
        {
            return direction switch
            {
                GridDirection.Up => Vector2Int.up,
                GridDirection.Right => Vector2Int.right,
                GridDirection.Down => Vector2Int.down,
                GridDirection.Left => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        /// <summary>Rotation in degrees so a sprite "facing up" by default points correctly.</summary>
        public static float ToZRotation(this GridDirection direction)
        {
            return direction switch
            {
                GridDirection.Up => 0f,
                GridDirection.Left => 90f,
                GridDirection.Down => 180f,
                GridDirection.Right => 270f,
                _ => 0f
            };
        }

        public static GridDirection FromInput(float x, float y)
        {
            // Prioritise the dominant axis to keep grid movement crisp (no diagonals).
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                return x > 0f ? GridDirection.Right : GridDirection.Left;
            }

            if (Mathf.Abs(y) > 0f)
            {
                return y > 0f ? GridDirection.Up : GridDirection.Down;
            }

            return GridDirection.None;
        }
    }
}
