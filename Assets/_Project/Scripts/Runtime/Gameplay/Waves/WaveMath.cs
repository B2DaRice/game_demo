using AstroIdle.Content;
using AstroIdle.Gameplay.Enemies;
using UnityEngine;

namespace AstroIdle.Gameplay.Waves
{
    /// <summary>
    /// Pure wave-difficulty formulas driven by <see cref="WaveConfig"/>. Documented with example numbers in
    /// docs/GAME_DESIGN.md — keep the two in sync when changing anything here.
    /// </summary>
    public static class WaveMath
    {
        /// <summary>Regular enemies in a wave (bosses are extra): min(base + perWave * (wave-1), max).</summary>
        public static int EnemyCount(WaveConfig config, int wave)
        {
            return Mathf.Min(config.BaseEnemyCount + config.EnemiesPerWave * (wave - 1), config.MaxEnemiesPerWave);
        }

        /// <summary>Seconds between spawns: max(min, base * multiplier^(wave-1)).</summary>
        public static float SpawnInterval(WaveConfig config, int wave)
        {
            return Mathf.Max(config.MinSpawnInterval, config.BaseSpawnInterval * Mathf.Pow(config.SpawnIntervalMultiplierPerWave, wave - 1));
        }

        public static bool IsBossWave(WaveConfig config, int wave)
        {
            return config.BossEveryNWaves > 0 && wave % config.BossEveryNWaves == 0;
        }

        /// <summary>Compounding health/damage/reward growth and capped linear speed growth for a wave.</summary>
        public static EnemyScaling Scaling(WaveConfig config, int wave)
        {
            int steps = Mathf.Max(0, wave - 1);
            return new EnemyScaling(
                Mathf.Pow(config.HealthGrowthPerWave, steps),
                Mathf.Pow(config.DamageGrowthPerWave, steps),
                System.Math.Pow(config.RewardGrowthPerWave, steps),
                Mathf.Min(1f + config.SpeedGrowthPerWave * steps, config.MaxSpeedMultiplier));
        }
    }
}
