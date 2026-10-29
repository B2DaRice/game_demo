using AstroIdle.Gameplay.Player;
using UnityEngine;

namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>Steers toward the player with limited acceleration (so it can be out-maneuvered) and rams it.</summary>
    public class ChaseBehaviour : IEnemyBehaviour
    {
        /// <summary>Acceleration as a multiple of the enemy's top speed per second.</summary>
        private const float AccelerationFactor = 2.5f;

        public void OnActivated(Enemy enemy)
        {
        }

        public void Tick(Enemy enemy, float deltaTime)
        {
            PlayerShip player = enemy.Context.Player;
            Vector2 target = player != null && player.IsAlive ? player.Position : enemy.Context.Arena.Center;

            Vector2 desired = (target - enemy.Position).normalized * enemy.Speed;
            enemy.Velocity = Vector2.MoveTowards(enemy.Velocity, desired, enemy.Speed * AccelerationFactor * deltaTime);
            enemy.Facing = enemy.Velocity;
            enemy.SetPosition(enemy.Position + enemy.Velocity * deltaTime);
        }
    }
}
