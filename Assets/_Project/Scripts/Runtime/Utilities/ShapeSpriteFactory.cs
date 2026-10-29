using System.Collections.Generic;
using UnityEngine;

namespace AstroIdle.Utilities
{
    /// <summary>
    /// Generates white, softly glowing vector-style sprites at runtime so the game needs no art assets
    /// (an "Asteroids" look). Sprites are 2 world units across at scale 1, i.e. radius 1:
    /// set transform.localScale = desired radius, and tint with SpriteRenderer.color.
    /// Sprites are built lazily on first request and cached for the session.
    /// </summary>
    public static class ShapeSpriteFactory
    {
        private const int TextureSize = 128;
        private const float ShapeScale = 0.8f;    // leaves room inside the texture for the glow
        private const float OutlineWidth = 0.07f; // normalized units (-1..1 space)
        private const float GlowFalloff = 0.10f;
        private const float GlowStrength = 0.45f;
        private const float HollowFillAlpha = 0.15f;

        private static readonly Dictionary<ShapeType, Sprite> Cache = new Dictionary<ShapeType, Sprite>();

        public static Sprite Get(ShapeType shape)
        {
            if (Cache.TryGetValue(shape, out Sprite sprite) && sprite != null) return sprite;
            sprite = Build(shape);
            Cache[shape] = sprite;
            return sprite;
        }

        // Static state survives play sessions when domain reload is disabled; reset it explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Cache.Clear();
        }

        private static Sprite Build(ShapeType shape)
        {
            Vector2[] polygon = GetPolygon(shape);
            float fillAlpha = IsSolid(shape) ? 1f : HollowFillAlpha;
            float pixelSize = 2f / TextureSize;
            var pixels = new Color32[TextureSize * TextureSize];

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    var point = new Vector2((x + 0.5f) * pixelSize - 1f, (y + 0.5f) * pixelSize - 1f);
                    float edgeDistance = DistanceToEdges(point, polygon);
                    bool inside = IsInside(point, polygon);

                    float outline = Mathf.Clamp01(1f - (edgeDistance - OutlineWidth * 0.5f) / pixelSize);
                    float fill = inside ? fillAlpha : 0f;
                    float glow = inside ? 0f : GlowStrength * Mathf.Exp(-edgeDistance / GlowFalloff);
                    float alpha = Mathf.Max(outline, Mathf.Max(fill, glow));

                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = "Shape_" + shape,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            var sprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), TextureSize / 2f);
            sprite.name = texture.name;
            return sprite;
        }

        private static bool IsSolid(ShapeType shape)
        {
            return shape == ShapeType.Dot || shape == ShapeType.Bolt || shape == ShapeType.Missile || shape == ShapeType.Spark;
        }

        /// <summary>Polygon outlines in -1..1 space. All shapes point "up" (+Y) as their forward direction.</summary>
        private static Vector2[] GetPolygon(ShapeType shape)
        {
            Vector2[] points;
            switch (shape)
            {
                case ShapeType.Ship:
                    points = new[] { new Vector2(0f, 1f), new Vector2(0.72f, -0.85f), new Vector2(0f, -0.45f), new Vector2(-0.72f, -0.85f) };
                    break;
                case ShapeType.Diamond:
                    points = new[] { new Vector2(0f, 1f), new Vector2(0.65f, 0f), new Vector2(0f, -1f), new Vector2(-0.65f, 0f) };
                    break;
                case ShapeType.Square:
                    points = new[] { new Vector2(-0.8f, -0.8f), new Vector2(0.8f, -0.8f), new Vector2(0.8f, 0.8f), new Vector2(-0.8f, 0.8f) };
                    break;
                case ShapeType.Hexagon:
                    points = RegularPolygon(6, 1f, 30f);
                    break;
                case ShapeType.Rock:
                    points = Rock();
                    break;
                case ShapeType.Saucer:
                    points = new[]
                    {
                        new Vector2(-1f, 0f), new Vector2(-0.5f, 0.3f), new Vector2(-0.25f, 0.6f), new Vector2(0.25f, 0.6f),
                        new Vector2(0.5f, 0.3f), new Vector2(1f, 0f), new Vector2(0.5f, -0.35f), new Vector2(-0.5f, -0.35f),
                    };
                    break;
                case ShapeType.Bolt:
                    points = new[] { new Vector2(-0.22f, -1f), new Vector2(0.22f, -1f), new Vector2(0.22f, 1f), new Vector2(-0.22f, 1f) };
                    break;
                case ShapeType.Missile:
                    points = new[] { new Vector2(0f, 1f), new Vector2(0.45f, -0.6f), new Vector2(0.2f, -1f), new Vector2(-0.2f, -1f), new Vector2(-0.45f, -0.6f) };
                    break;
                case ShapeType.Spark:
                    points = new[] { new Vector2(-0.15f, -1f), new Vector2(0.15f, -1f), new Vector2(0.15f, 1f), new Vector2(-0.15f, 1f) };
                    break;
                case ShapeType.Circle:
                case ShapeType.Dot:
                default:
                    points = RegularPolygon(32, 1f, 0f);
                    break;
            }

            for (int i = 0; i < points.Length; i++) points[i] *= ShapeScale;
            return points;
        }

        private static Vector2[] RegularPolygon(int sides, float radius, float rotationDegrees)
        {
            var points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (rotationDegrees + 360f * i / sides) * Mathf.Deg2Rad;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return points;
        }

        private static Vector2[] Rock()
        {
            // Fixed radii keep the rock deterministic; spin and scale give variety in game.
            float[] radii = { 1f, 0.82f, 0.95f, 0.7f, 0.9f, 1f, 0.78f, 0.92f, 0.85f, 0.75f, 0.97f };
            var points = new Vector2[radii.Length];
            for (int i = 0; i < radii.Length; i++)
            {
                float angle = 360f * i / radii.Length * Mathf.Deg2Rad;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radii[i];
            }
            return points;
        }

        private static float DistanceToEdges(Vector2 point, Vector2[] polygon)
        {
            float min = float.MaxValue;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                min = Mathf.Min(min, DistanceToSegment(point, polygon[j], polygon[i]));
            }
            return min;
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
            return Vector2.Distance(p, a + ab * t);
        }

        private static bool IsInside(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
