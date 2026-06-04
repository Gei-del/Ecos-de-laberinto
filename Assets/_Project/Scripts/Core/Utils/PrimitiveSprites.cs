using System.Collections.Generic;
using UnityEngine;

namespace EcosDelLaberinto.Core.Utils
{
    /// <summary>
    /// Generates simple solid-colour sprites at runtime so the game is fully playable with
    /// placeholder art (no imported textures required). Production art simply replaces the sprites
    /// assigned in the prefabs/data — no code changes needed.
    /// </summary>
    public static class PrimitiveSprites
    {
        private const float PixelsPerUnit = 32f;
        private static readonly Dictionary<string, Sprite> Cache = new();

        public static Sprite Square()
        {
            return GetOrCreate("square", size: 32, circle: false);
        }

        public static Sprite Circle()
        {
            return GetOrCreate("circle", size: 32, circle: true);
        }

        public static Sprite Diamond()
        {
            const string key = "diamond";
            if (Cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            const int size = 32;
            var tex = NewTexture(size);
            var half = size / 2f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var manhattan = Mathf.Abs(x + 0.5f - half) + Mathf.Abs(y + 0.5f - half);
                    tex.SetPixel(x, y, manhattan <= half ? Color.white : Color.clear);
                }
            }

            return Finalize(tex, key);
        }

        private static Sprite GetOrCreate(string key, int size, bool circle)
        {
            if (Cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var tex = NewTexture(size);
            var center = size / 2f;
            var radius = size / 2f - 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if (!circle)
                    {
                        tex.SetPixel(x, y, Color.white);
                        continue;
                    }

                    var dx = x + 0.5f - center;
                    var dy = y + 0.5f - center;
                    tex.SetPixel(x, y, dx * dx + dy * dy <= radius * radius ? Color.white : Color.clear);
                }
            }

            return Finalize(tex, key);
        }

        private static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        private static Sprite Finalize(Texture2D tex, string key)
        {
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.name = $"Primitive_{key}";
            Cache[key] = sprite;
            return sprite;
        }
    }
}
