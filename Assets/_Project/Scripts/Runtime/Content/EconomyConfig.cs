using System.Collections.Generic;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// Global economy and idle knobs: kill income multiplier, weapon unlock pricing, shared upgrade cost curves,
    /// offline earnings, income smoothing and autosave. Exactly one must exist in Resources/Content/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Economy Config", fileName = "EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("New Game")]
        [SerializeField] private double startingCredits = 0d;

        [Header("Income")]
        [Tooltip("Multiplies every kill reward (after wave scaling, before the ship's credit multiplier). The global income dial.")]
        [SerializeField] private float killRewardMultiplier = 1f;

        [Header("Weapon Unlocks")]
        [Tooltip("Credits to unlock any weapon (same price for every weapon).")]
        [SerializeField] private double weaponUnlockCost = 15d;
        [Tooltip("Unlock cost multiplier per weapon already owned beyond the first. 1 = flat price; 2 = each extra weapon doubles.")]
        [SerializeField] private float weaponUnlockCostGrowth = 1f;

        [Header("Shared Upgrade Cost Curves")]
        [Tooltip("Referenced by UpgradeTrack.costCurveId so equivalent upgrades cost the same across weapons.")]
        [SerializeField] private List<CostCurve> upgradeCostCurves = new List<CostCurve>();

        [Header("Offline Earnings")]
        [Tooltip("Fraction of the recent live credits/sec earned while the app is closed.")]
        [SerializeField] private float offlineEfficiency = 0.25f;
        [SerializeField] private float maxOfflineHours = 8f;
        [Tooltip("Absences shorter than this grant nothing (avoids popups on quick app switches).")]
        [SerializeField] private float minOfflineSeconds = 120f;

        [Header("Tracking")]
        [Tooltip("Smoothing window for the live credits/sec estimate.")]
        [SerializeField] private float creditRateWindowSeconds = 60f;
        [SerializeField] private float autosaveIntervalSeconds = 15f;

        public double StartingCredits => startingCredits;
        public float KillRewardMultiplier => killRewardMultiplier;
        public double WeaponUnlockCost => weaponUnlockCost;
        public float WeaponUnlockCostGrowth => weaponUnlockCostGrowth;
        public IReadOnlyList<CostCurve> UpgradeCostCurves => upgradeCostCurves;
        public float OfflineEfficiency => offlineEfficiency;
        public float MaxOfflineHours => maxOfflineHours;
        public float MinOfflineSeconds => minOfflineSeconds;
        public float CreditRateWindowSeconds => creditRateWindowSeconds;
        public float AutosaveIntervalSeconds => autosaveIntervalSeconds;

        /// <returns>The curve with this id, or null.</returns>
        public CostCurve GetCostCurve(string curveId)
        {
            if (string.IsNullOrEmpty(curveId)) return null;
            for (int i = 0; i < upgradeCostCurves.Count; i++)
            {
                if (upgradeCostCurves[i].Id == curveId) return upgradeCostCurves[i];
            }
            return null;
        }
    }
}
