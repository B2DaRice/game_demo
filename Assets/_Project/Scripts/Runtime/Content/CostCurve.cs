using System;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// A named, shared cost curve (defined in EconomyConfig). Upgrade tracks that set a costCurveId use it
    /// instead of their own baseCost/costGrowth, so every weapon's "Damage" upgrade can cost exactly the same
    /// and be re-priced from one place.
    /// </summary>
    [Serializable]
    public class CostCurve
    {
        [Tooltip("Referenced by UpgradeTrack.costCurveId.")]
        [SerializeField] private string id = "new_curve";
        [SerializeField] private double baseCost = 10d;
        [Tooltip("Cost multiplier per level owned. Must be >= 1.")]
        [SerializeField] private float costGrowth = 1.2f;

        public string Id => id;
        public double BaseCost => baseCost;
        public float CostGrowth => costGrowth;
    }
}
