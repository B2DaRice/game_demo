namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>
    /// Enemy AI strategy. Implementations are STATELESS singletons shared by every enemy using them;
    /// keep per-enemy state on <see cref="Enemy"/> (Velocity, FireTimer, OrbitDirection, ...).
    /// Behaviours are responsible for moving the enemy (Enemy.SetPosition) and setting Facing.
    /// </summary>
    public interface IEnemyBehaviour
    {
        /// <summary>Called once right after the enemy is activated/spawned.</summary>
        void OnActivated(Enemy enemy);

        /// <summary>Called every frame while the enemy is active.</summary>
        void Tick(Enemy enemy, float deltaTime);
    }
}
