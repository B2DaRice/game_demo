using System;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Pure balance formulas shared by progression systems. Kept free of Unity types so they are trivially
    /// unit-testable. See docs/GAME_DESIGN.md for the reasoning behind each formula.
    /// </summary>
    public static class ProgressionMath
    {
        /// <summary>Cost of buying the next level: ceil(baseCost * growth ^ currentLevel).</summary>
        public static double UpgradeCost(double baseCost, float growth, int currentLevel)
        {
            // Convert via decimal so 1.2f becomes exactly 1.2 (not 1.2000000477), and subtract a tiny
            // epsilon before rounding up so float noise like 12.0000001 doesn't become 13.
            double exactGrowth = (double)(decimal)growth;
            double raw = baseCost * Math.Pow(exactGrowth, Math.Max(0, currentLevel));
            return Math.Ceiling(raw - 1e-6);
        }

        /// <summary>
        /// Price to unlock another weapon: ceil(baseCost * growth ^ (ownedCount - 1)).
        /// With growth 1 every weapon costs the same; above 1 each extra weapon gets pricier.
        /// </summary>
        public static double WeaponUnlockCost(double baseCost, float growth, int ownedCount)
        {
            return UpgradeCost(baseCost, growth, Math.Max(0, ownedCount - 1));
        }

        /// <summary>Maps a random roll in [0, 1) to an index in [0, count). Returns -1 when count is 0.</summary>
        public static int PickIndex(int count, float roll01)
        {
            if (count <= 0) return -1;
            return Math.Min(count - 1, Math.Max(0, (int)(roll01 * count)));
        }

        /// <summary>Final stat value: (base + flat) * (1 + percent).</summary>
        public static float EvaluateStat(float baseValue, float flat, float percent)
        {
            return (baseValue + flat) * (1f + percent);
        }
    }
}
