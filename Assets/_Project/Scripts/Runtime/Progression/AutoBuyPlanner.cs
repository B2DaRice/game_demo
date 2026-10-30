using System.Collections.Generic;
using AstroIdle.Content;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Pure decision rules for auto mode (unit tested). The rule: among all candidates, pick the one with the
    /// lowest cost ÷ weight for the active profile. Higher weight = treated as cheaper = bought sooner. Because
    /// every purchase raises that track's next price, spending naturally spreads across categories roughly in
    /// proportion to their weights.
    /// </summary>
    public static class AutoBuyPlanner
    {
        /// <returns>Index of the best candidate, or -1 if none has a positive weight.</returns>
        public static int PickBest(IReadOnlyList<AutoBuyCandidate> candidates, AutoBuyProfile profile)
        {
            int best = -1;
            double bestScore = double.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                float weight = profile.GetWeight(candidates[i].Category);
                if (weight <= 0f) continue;

                double score = candidates[i].Cost / weight;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }
            return best;
        }

        /// <summary>Which auto mode bucket an upgrade on the ship (or a weapon) belongs to.</summary>
        public static AutoBuyCategory CategoryFor(StatId stat, bool isShipTrack)
        {
            if (!isShipTrack) return AutoBuyCategory.WeaponUpgrade;

            switch (stat)
            {
                case StatId.CreditMultiplier:
                    return AutoBuyCategory.ShipEconomy;
                case StatId.GlobalDamageMultiplier:
                case StatId.GlobalFireRateMultiplier:
                    return AutoBuyCategory.ShipOffense;
                case StatId.MaxHull:
                case StatId.HullRegen:
                case StatId.MoveSpeed:
                default:
                    return AutoBuyCategory.ShipDefense;
            }
        }
    }
}
