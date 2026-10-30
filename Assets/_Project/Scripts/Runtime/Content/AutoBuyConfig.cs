using System.Collections.Generic;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// Auto mode tuning: how often it shops and the spending profile for each strategy (Attack / Balanced /
    /// Defense). Exactly one must exist in Resources/Content/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Auto Buy Config", fileName = "AutoBuyConfig")]
    public class AutoBuyConfig : ScriptableObject
    {
        [Tooltip("Real-time seconds between auto mode shopping passes.")]
        [SerializeField] private float tickIntervalSeconds = 0.5f;
        [Tooltip("Safety cap on purchases in one pass.")]
        [SerializeField] private int maxPurchasesPerTick = 10;
        [SerializeField] private List<AutoBuyProfile> profiles = new List<AutoBuyProfile>();

        public float TickIntervalSeconds => tickIntervalSeconds;
        public int MaxPurchasesPerTick => maxPurchasesPerTick;
        public IReadOnlyList<AutoBuyProfile> Profiles => profiles;

        /// <returns>The profile for the strategy, or null if it isn't configured.</returns>
        public AutoBuyProfile GetProfile(AutoBuyStrategy strategy)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Strategy == strategy) return profiles[i];
            }
            return null;
        }
    }
}
