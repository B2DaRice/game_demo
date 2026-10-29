using UnityEngine;

namespace AstroIdle.Gameplay.Arena
{
    /// <summary>
    /// World-space bounds of the play field, derived from the camera. Provides spawn points, screen wrapping
    /// and clamping helpers. Recalculate() whenever the screen size / safe area changes.
    /// </summary>
    public class PlayArea
    {
        /// <summary>How far outside the visible area enemies spawn.</summary>
        public const float SpawnMargin = 1.2f;

        private const float ShipPadding = 0.4f;

        /// <summary>The full visible world rectangle.</summary>
        public Rect Bounds { get; private set; }

        /// <summary>Where the player ship may move: the visible area minus padding and the HUD strip.</summary>
        public Rect ShipBounds { get; private set; }

        public Vector2 Center => ShipBounds.center;

        public void Recalculate(Camera camera, float hudWorldHeight)
        {
            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector2 center = camera.transform.position;

            Bounds = new Rect(center.x - halfWidth, center.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
            ShipBounds = new Rect(
                Bounds.xMin + ShipPadding,
                Bounds.yMin + ShipPadding,
                Bounds.width - ShipPadding * 2f,
                Mathf.Max(1f, Bounds.height - ShipPadding * 2f - hudWorldHeight));
        }

        public bool Contains(Vector2 point) => Bounds.Contains(point);

        public bool IsFarOutside(Vector2 point, float margin)
        {
            Rect b = Bounds;
            return point.x < b.xMin - margin || point.x > b.xMax + margin ||
                   point.y < b.yMin - margin || point.y > b.yMax + margin;
        }

        public Vector2 ClampToShipBounds(Vector2 point)
        {
            Rect b = ShipBounds;
            return new Vector2(Mathf.Clamp(point.x, b.xMin, b.xMax), Mathf.Clamp(point.y, b.yMin, b.yMax));
        }

        /// <summary>Asteroids-style wrap: leaving one edge (plus margin) re-enters from the opposite edge.</summary>
        public Vector2 Wrap(Vector2 point, float margin)
        {
            Rect b = Bounds;
            if (point.x > b.xMax + margin) point.x = b.xMin - margin;
            else if (point.x < b.xMin - margin) point.x = b.xMax + margin;
            if (point.y > b.yMax + margin) point.y = b.yMin - margin;
            else if (point.y < b.yMin - margin) point.y = b.yMax + margin;
            return point;
        }

        /// <summary>Random point just outside the visible area, chosen proportionally to edge length.</summary>
        public Vector2 RandomSpawnPoint()
        {
            Rect b = Bounds;
            float perimeter = 2f * (b.width + b.height);
            float roll = Random.value * perimeter;

            if (roll < b.width) return new Vector2(b.xMin + roll, b.yMax + SpawnMargin);
            roll -= b.width;
            if (roll < b.width) return new Vector2(b.xMin + roll, b.yMin - SpawnMargin);
            roll -= b.width;
            if (roll < b.height) return new Vector2(b.xMin - SpawnMargin, b.yMin + roll);
            roll -= b.height;
            return new Vector2(b.xMax + SpawnMargin, b.yMin + Mathf.Min(roll, b.height));
        }
    }
}
