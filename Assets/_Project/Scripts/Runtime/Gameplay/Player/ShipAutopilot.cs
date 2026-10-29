using AstroIdle.Gameplay.Arena;
using AstroIdle.Gameplay.Combat;
using AstroIdle.Gameplay.Enemies;
using UnityEngine;

namespace AstroIdle.Gameplay.Player
{
    /// <summary>
    /// Idle-game "pilot": combines simple steering forces into a desired velocity.
    ///  - Avoid: push away from nearby enemies (stronger the closer they are; bosses count double).
    ///  - Center: gentle pull toward the arena center so the ship doesn't hug walls.
    ///  - Walls: strong push when inside the wall margin.
    ///  - Wander: slow Perlin-noise drift so the ship keeps moving when nothing is around.
    /// Tune the weights below to change how the ship "feels".
    /// </summary>
    public class ShipAutopilot
    {
        private const float DangerRadius = 3.2f;
        private const float AvoidWeight = 1.6f;
        private const float BossAvoidMultiplier = 2f;
        private const float CenterWeight = 0.7f;
        private const float WallMargin = 1.2f;
        private const float WallWeight = 2f;
        private const float WanderWeight = 0.35f;
        private const float WanderFrequency = 0.3f;
        private const float Acceleration = 8f;

        private readonly float noiseSeed = Random.value * 100f;
        private float noiseTime;

        public Vector2 ComputeVelocity(Vector2 position, Vector2 currentVelocity, float maxSpeed, CombatRegistry registry, PlayArea arena, float deltaTime)
        {
            Vector2 steer = Vector2.zero;

            var enemies = registry.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                Vector2 away = position - enemy.Position;
                float distance = away.magnitude - enemy.Radius;
                if (distance >= DangerRadius) continue;

                float closeness = 1f - Mathf.Clamp01(distance / DangerRadius);
                float weight = closeness * closeness * AvoidWeight * (enemy.Definition.IsBoss ? BossAvoidMultiplier : 1f);
                steer += (away.sqrMagnitude > 0.0001f ? away.normalized : Random.insideUnitCircle.normalized) * weight;
            }

            Rect bounds = arena.ShipBounds;
            steer += (arena.Center - position) / Mathf.Max(bounds.width, 0.01f) * CenterWeight;

            if (position.x - bounds.xMin < WallMargin) steer.x += (1f - (position.x - bounds.xMin) / WallMargin) * WallWeight;
            if (bounds.xMax - position.x < WallMargin) steer.x -= (1f - (bounds.xMax - position.x) / WallMargin) * WallWeight;
            if (position.y - bounds.yMin < WallMargin) steer.y += (1f - (position.y - bounds.yMin) / WallMargin) * WallWeight;
            if (bounds.yMax - position.y < WallMargin) steer.y -= (1f - (bounds.yMax - position.y) / WallMargin) * WallWeight;

            noiseTime += deltaTime * WanderFrequency;
            var wander = new Vector2(
                Mathf.PerlinNoise(noiseSeed, noiseTime) - 0.5f,
                Mathf.PerlinNoise(noiseSeed + 37f, noiseTime) - 0.5f) * 2f;
            steer += wander * WanderWeight;

            Vector2 desired = Vector2.ClampMagnitude(steer, 1f) * maxSpeed;
            return Vector2.MoveTowards(currentVelocity, desired, Acceleration * deltaTime);
        }
    }
}
