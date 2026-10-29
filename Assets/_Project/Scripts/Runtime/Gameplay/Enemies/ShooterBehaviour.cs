using AstroIdle.Content;
using AstroIdle.Gameplay.Player;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>
    /// Ranged attacker: approaches to its preferred range, circles the player, and fires volleys on an
    /// interval. Stays on screen once it has entered the arena.
    /// </summary>
    public class ShooterBehaviour : IEnemyBehaviour
    {
        private const float RangeTolerance = 0.5f;
        private const float OrbitWeight = 0.6f;
        private const float AccelerationFactor = 2f;
        private const float MaxFireRangeFactor = 1.5f;
        private const float ProjectileSize = 0.12f;

        public void OnActivated(Enemy enemy)
        {
            enemy.OrbitDirection = Random.value < 0.5f ? -1f : 1f;
            enemy.FireTimer = enemy.Definition.FireInterval * Random.Range(0.5f, 1f);
        }

        public void Tick(Enemy enemy, float deltaTime)
        {
            EnemyDefinition definition = enemy.Definition;
            PlayerShip player = enemy.Context.Player;
            bool hasTarget = player != null && player.IsAlive;
            Vector2 target = hasTarget ? player.Position : enemy.Context.Arena.Center;

            Vector2 toTarget = target - enemy.Position;
            float distance = toTarget.magnitude;
            Vector2 direction = distance > 0.001f ? toTarget / distance : Vector2.up;

            float radial = 0f;
            if (distance > definition.PreferredRange + RangeTolerance) radial = 1f;
            else if (distance < definition.PreferredRange - RangeTolerance) radial = -1f;

            Vector2 tangent = new Vector2(-direction.y, direction.x) * enemy.OrbitDirection * OrbitWeight;
            Vector2 desired = (direction * radial + tangent).normalized * enemy.Speed;
            enemy.Velocity = Vector2.MoveTowards(enemy.Velocity, desired, enemy.Speed * AccelerationFactor * deltaTime);

            Vector2 position = enemy.Position + enemy.Velocity * deltaTime;
            if (!enemy.HasEnteredArena)
            {
                if (enemy.Context.Arena.Contains(position)) enemy.HasEnteredArena = true;
            }
            else
            {
                // Same region as the ship: keeps shooters visible and out from under the HUD.
                position = enemy.Context.Arena.ClampToShipBounds(position);
            }

            enemy.Facing = direction;
            enemy.SetPosition(position);

            enemy.FireTimer -= deltaTime;
            if (enemy.FireTimer > 0f) return;
            if (!hasTarget || !enemy.HasEnteredArena || distance > definition.PreferredRange * MaxFireRangeFactor) return;

            enemy.FireTimer = definition.FireInterval;
            Fire(enemy, direction);
        }

        private static void Fire(Enemy enemy, Vector2 aim)
        {
            EnemyDefinition definition = enemy.Definition;
            int count = Mathf.Max(1, definition.ProjectilesPerShot);
            float damage = definition.ProjectileDamage * enemy.DamageMultiplier;

            for (int i = 0; i < count; i++)
            {
                Vector2 direction = VectorUtils.Rotate(aim, VectorUtils.SpreadOffset(i, count, definition.ShotSpreadDegrees));
                Vector2 origin = enemy.Position + direction * enemy.Radius;
                enemy.Context.Projectiles.FireEnemy(origin, direction, definition.ProjectileSpeed, damage, definition.Color, ProjectileSize);
            }
        }
    }
}
