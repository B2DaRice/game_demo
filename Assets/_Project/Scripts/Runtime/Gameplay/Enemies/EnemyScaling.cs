namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>Per-wave multipliers applied to an EnemyDefinition's base stats (see WaveMath.Scaling).</summary>
    public readonly struct EnemyScaling
    {
        public static readonly EnemyScaling Identity = new EnemyScaling(1f, 1f, 1d, 1f);

        public EnemyScaling(float healthMultiplier, float damageMultiplier, double rewardMultiplier, float speedMultiplier)
        {
            HealthMultiplier = healthMultiplier;
            DamageMultiplier = damageMultiplier;
            RewardMultiplier = rewardMultiplier;
            SpeedMultiplier = speedMultiplier;
        }

        public float HealthMultiplier { get; }
        public float DamageMultiplier { get; }
        public double RewardMultiplier { get; }
        public float SpeedMultiplier { get; }
    }
}
