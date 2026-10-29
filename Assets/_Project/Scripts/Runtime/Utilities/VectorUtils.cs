using UnityEngine;

namespace AstroIdle.Utilities
{
    /// <summary>Small 2D math helpers shared by gameplay code.</summary>
    public static class VectorUtils
    {
        /// <summary>Rotates a vector counter-clockwise by the given angle in degrees.</summary>
        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        /// <summary>Z rotation that makes a sprite pointing up (+Y) face the given direction.</summary>
        public static Quaternion FacingRotation(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            return Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>Evenly spreads <paramref name="index"/> of <paramref name="count"/> across a cone of <paramref name="spreadDegrees"/>.</summary>
        public static float SpreadOffset(int index, int count, float spreadDegrees)
        {
            if (count <= 1) return 0f;
            return Mathf.Lerp(-spreadDegrees * 0.5f, spreadDegrees * 0.5f, index / (float)(count - 1));
        }
    }
}
