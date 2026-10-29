namespace AstroIdle.Content
{
    /// <summary>What a milestone measures. Serialized as ints: NEVER reorder, only append.</summary>
    public enum MilestoneCondition
    {
        /// <summary>Highest wave ever started (reaching wave 11 = clearing wave 10).</summary>
        ReachWave = 0,

        /// <summary>Lifetime enemies destroyed by the player.</summary>
        TotalKills = 1,

        /// <summary>Lifetime credits earned (kills, milestone rewards and offline earnings).</summary>
        TotalCreditsEarned = 2,
    }
}
