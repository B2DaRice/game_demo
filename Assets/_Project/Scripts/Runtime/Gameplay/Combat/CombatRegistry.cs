using System.Collections.Generic;
using AstroIdle.Gameplay.Enemies;
using AstroIdle.Gameplay.Player;
using UnityEngine;

namespace AstroIdle.Gameplay.Combat
{
    /// <summary>
    /// Tracks live combatants and answers spatial queries (nearest enemy, overlaps, radius searches).
    /// Collision is simple circle-vs-circle math instead of Unity physics: cheap and predictable for the
    /// entity counts this game uses (&lt; ~100 enemies). If counts grow a lot, add a spatial grid here.
    /// </summary>
    public class CombatRegistry
    {
        private readonly List<Enemy> enemies = new List<Enemy>(128);

        public IReadOnlyList<Enemy> Enemies => enemies;
        public int EnemyCount => enemies.Count;
        public PlayerShip Player { get; set; }

        public void Register(Enemy enemy) => enemies.Add(enemy);

        public void Unregister(Enemy enemy) => enemies.Remove(enemy);

        /// <returns>The closest active enemy within <paramref name="maxRange"/>, or null.</returns>
        public Enemy FindNearest(Vector2 from, float maxRange)
        {
            Enemy best = null;
            float bestSqr = maxRange * maxRange;
            for (int i = 0; i < enemies.Count; i++)
            {
                float sqr = (enemies[i].Position - from).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = enemies[i];
                }
            }
            return best;
        }

        /// <summary>
        /// Swept circle test: the enemy hit first by a circle of <paramref name="radius"/> moving from
        /// <paramref name="from"/> to <paramref name="to"/> this frame, skipping <paramref name="ignore"/>.
        /// Sweeping (instead of testing only the end point) stops fast projectiles tunnelling through small enemies.
        /// </summary>
        /// <param name="hitPoint">Point along the path closest to the hit enemy's center.</param>
        public Enemy FindFirstOverlap(Vector2 from, Vector2 to, float radius, HashSet<Enemy> ignore, out Vector2 hitPoint)
        {
            Enemy best = null;
            float bestT = float.MaxValue;
            hitPoint = to;

            Vector2 path = to - from;
            float pathSqr = Mathf.Max(path.sqrMagnitude, 1e-8f);
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (ignore != null && ignore.Contains(enemy)) continue;

                float t = Mathf.Clamp01(Vector2.Dot(enemy.Position - from, path) / pathSqr);
                Vector2 closest = from + path * t;
                float combined = radius + enemy.Radius;
                if ((enemy.Position - closest).sqrMagnitude <= combined * combined && t < bestT)
                {
                    bestT = t;
                    best = enemy;
                    hitPoint = closest;
                }
            }
            return best;
        }

        /// <summary>Fills <paramref name="results"/> with every enemy whose collision circle touches the area.</summary>
        public void FindInRadius(Vector2 center, float radius, List<Enemy> results)
        {
            results.Clear();
            for (int i = 0; i < enemies.Count; i++)
            {
                float combined = radius + enemies[i].Radius;
                if ((enemies[i].Position - center).sqrMagnitude <= combined * combined) results.Add(enemies[i]);
            }
        }
    }
}
