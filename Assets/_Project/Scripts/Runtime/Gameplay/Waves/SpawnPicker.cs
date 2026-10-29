using System.Collections.Generic;
using AstroIdle.Content;

namespace AstroIdle.Gameplay.Waves
{
    /// <summary>Weighted random selection from the spawn table, filtered by wave.</summary>
    public static class SpawnPicker
    {
        /// <param name="roll01">A random value in [0, 1). Injected so the selection is testable.</param>
        /// <returns>The chosen enemy id, or null when no entry is eligible for this wave.</returns>
        public static string Pick(IReadOnlyList<SpawnEntry> table, int wave, float roll01)
        {
            float totalWeight = 0f;
            for (int i = 0; i < table.Count; i++)
            {
                if (table[i].IsEligible(wave)) totalWeight += table[i].Weight;
            }
            if (totalWeight <= 0f) return null;

            float roll = roll01 * totalWeight;
            string lastEligible = null;
            for (int i = 0; i < table.Count; i++)
            {
                SpawnEntry entry = table[i];
                if (!entry.IsEligible(wave)) continue;
                lastEligible = entry.EnemyId;
                if (roll < entry.Weight) return entry.EnemyId;
                roll -= entry.Weight;
            }
            return lastEligible; // floating point safety net
        }
    }
}
