using System.Collections.Generic;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// All wave pacing and difficulty-scaling knobs. Formulas live in Gameplay/Waves/WaveMath.cs and are
    /// documented in docs/GAME_DESIGN.md. Exactly one must exist in Resources/Content/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Wave Config", fileName = "WaveConfig")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Enemy Count")]
        [SerializeField] private int baseEnemyCount = 8;
        [SerializeField] private int enemiesPerWave = 2;
        [SerializeField] private int maxEnemiesPerWave = 80;
        [Tooltip("Spawning pauses while this many enemies are alive.")]
        [SerializeField] private int maxConcurrentEnemies = 30;

        [Header("Spawn Timing")]
        [SerializeField] private float baseSpawnInterval = 1.1f;
        [Tooltip("Interval is multiplied by this each wave (< 1 = faster spawns).")]
        [SerializeField] private float spawnIntervalMultiplierPerWave = 0.96f;
        [SerializeField] private float minSpawnInterval = 0.3f;
        [SerializeField] private float intermissionSeconds = 2f;

        [Header("Per-Wave Scaling (compounding: value * growth^(wave-1))")]
        [SerializeField] private float healthGrowthPerWave = 1.15f;
        [SerializeField] private float damageGrowthPerWave = 1.07f;
        [SerializeField] private float rewardGrowthPerWave = 1.13f;
        [Tooltip("Enemy speed multiplier on wave 1 (< 1 = a calmer opening).")]
        [SerializeField] private float startingSpeedMultiplier = 0.8f;
        [Tooltip("Linear: speed * (startingSpeedMultiplier + value * (wave-1)), capped by maxSpeedMultiplier.")]
        [SerializeField] private float speedGrowthPerWave = 0.02f;
        [SerializeField] private float maxSpeedMultiplier = 1.6f;

        [Header("Bosses")]
        [Tooltip("Every Nth wave ends with a boss. 0 = no bosses.")]
        [SerializeField] private int bossEveryNWaves = 10;
        [SerializeField] private string bossEnemyId = "mothership";

        [Header("Failure")]
        [Tooltip("Waves the player is pushed back when the ship is destroyed.")]
        [SerializeField] private int failWaveSetback = 1;

        [Header("Spawn Table")]
        [SerializeField] private List<SpawnEntry> spawnTable = new List<SpawnEntry>();

        public int BaseEnemyCount => baseEnemyCount;
        public int EnemiesPerWave => enemiesPerWave;
        public int MaxEnemiesPerWave => maxEnemiesPerWave;
        public int MaxConcurrentEnemies => maxConcurrentEnemies;
        public float BaseSpawnInterval => baseSpawnInterval;
        public float SpawnIntervalMultiplierPerWave => spawnIntervalMultiplierPerWave;
        public float MinSpawnInterval => minSpawnInterval;
        public float IntermissionSeconds => intermissionSeconds;
        public float HealthGrowthPerWave => healthGrowthPerWave;
        public float DamageGrowthPerWave => damageGrowthPerWave;
        public float RewardGrowthPerWave => rewardGrowthPerWave;
        public float StartingSpeedMultiplier => startingSpeedMultiplier;
        public float SpeedGrowthPerWave => speedGrowthPerWave;
        public float MaxSpeedMultiplier => maxSpeedMultiplier;
        public int BossEveryNWaves => bossEveryNWaves;
        public string BossEnemyId => bossEnemyId;
        public int FailWaveSetback => failWaveSetback;
        public IReadOnlyList<SpawnEntry> SpawnTable => spawnTable;
    }
}
