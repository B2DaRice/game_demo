using AstroIdle.Content;

namespace AstroIdle.Progression
{
    /// <summary>One thing auto mode could buy right now: its category and current price.</summary>
    public readonly struct AutoBuyCandidate
    {
        public AutoBuyCandidate(AutoBuyCategory category, double cost)
        {
            Category = category;
            Cost = cost;
        }

        public AutoBuyCategory Category { get; }
        public double Cost { get; }
    }
}
