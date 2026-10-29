using System;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>One row of WaveConfig's weighted spawn table.</summary>
    [Serializable]
    public class SpawnEntry
    {
        [SerializeField] private string enemyId = "";
        [Tooltip("First wave this enemy can appear in.")]
        [SerializeField] private int minWave = 1;
        [Tooltip("Last wave this enemy can appear in. 0 = no limit.")]
        [SerializeField] private int maxWave;
        [Tooltip("Relative chance vs. other eligible entries.")]
        [SerializeField] private float weight = 1f;

        public SpawnEntry()
        {
        }

        public SpawnEntry(string enemyId, int minWave, int maxWave, float weight)
        {
            this.enemyId = enemyId;
            this.minWave = minWave;
            this.maxWave = maxWave;
            this.weight = weight;
        }

        public string EnemyId => enemyId;
        public int MinWave => minWave;
        public int MaxWave => maxWave;
        public float Weight => weight;

        public bool IsEligible(int wave)
        {
            return wave >= minWave && (maxWave <= 0 || wave <= maxWave) && weight > 0f;
        }
    }
}
