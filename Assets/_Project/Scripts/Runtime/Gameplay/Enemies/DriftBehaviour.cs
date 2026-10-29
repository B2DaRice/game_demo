using UnityEngine;

namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>
    /// Classic asteroid: picks a heading through the middle of the arena, flies straight, and wraps around
    /// the screen edges once it has entered the visible area.
    /// </summary>
    public class DriftBehaviour : IEnemyBehaviour
    {
        private const float TargetJitterRadius = 2.5f;

        public void OnActivated(Enemy enemy)
        {
            // Split children arrive with a velocity already set; only fresh spawns pick a heading.
            if (enemy.Velocity != Vector2.zero) return;

            Vector2 target = enemy.Context.Arena.Center + Random.insideUnitCircle * TargetJitterRadius;
            Vector2 direction = (target - enemy.Position).normalized;
            enemy.Velocity = direction * enemy.Speed;
        }

        public void Tick(Enemy enemy, float deltaTime)
        {
            Vector2 position = enemy.Position + enemy.Velocity * deltaTime;

            if (!enemy.HasEnteredArena)
            {
                if (enemy.Context.Arena.Contains(position)) enemy.HasEnteredArena = true;
            }
            else
            {
                position = enemy.Context.Arena.Wrap(position, enemy.Radius);
            }

            enemy.Facing = enemy.Velocity;
            enemy.SetPosition(position);
        }
    }
}
