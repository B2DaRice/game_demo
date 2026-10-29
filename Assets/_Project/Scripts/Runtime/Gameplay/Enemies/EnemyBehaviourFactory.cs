using AstroIdle.Content;

namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>
    /// Maps <see cref="EnemyBehaviourType"/> to its behaviour. To add a new AI: append a value to the enum,
    /// implement IEnemyBehaviour, and add a case here.
    /// </summary>
    public static class EnemyBehaviourFactory
    {
        private static readonly IEnemyBehaviour Drift = new DriftBehaviour();
        private static readonly IEnemyBehaviour Chase = new ChaseBehaviour();
        private static readonly IEnemyBehaviour Shooter = new ShooterBehaviour();

        public static IEnemyBehaviour Get(EnemyBehaviourType type)
        {
            switch (type)
            {
                case EnemyBehaviourType.Chase: return Chase;
                case EnemyBehaviourType.Shooter: return Shooter;
                case EnemyBehaviourType.Drift:
                default: return Drift;
            }
        }
    }
}
