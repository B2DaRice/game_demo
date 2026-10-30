using System;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// Spending priorities for one auto mode strategy. Auto mode always buys the candidate with the lowest
    /// cost ÷ weight, so a category with weight 2 is bought as if it cost half as much. If that pick isn't
    /// affordable yet, auto mode saves up for it instead of buying something less important.
    /// Weight 0 = never buy that category automatically.
    /// </summary>
    [Serializable]
    public class AutoBuyProfile
    {
        [SerializeField] private AutoBuyStrategy strategy = AutoBuyStrategy.Balanced;
        [SerializeField] private string displayName = "Balanced";
        [Tooltip("Shown in the auto mode dialog.")]
        [SerializeField] private string description = "";

        [Header("Weights (relative priority)")]
        [SerializeField] private float weaponUpgradeWeight = 1f;
        [SerializeField] private float weaponUnlockWeight = 1f;
        [SerializeField] private float shipOffenseWeight = 0.6f;
        [SerializeField] private float shipDefenseWeight = 0.6f;
        [SerializeField] private float shipEconomyWeight = 0.6f;

        public AutoBuyProfile()
        {
        }

        public AutoBuyProfile(AutoBuyStrategy strategy, float weaponUpgrade, float weaponUnlock, float shipOffense, float shipDefense, float shipEconomy)
        {
            this.strategy = strategy;
            displayName = strategy.ToString();
            weaponUpgradeWeight = weaponUpgrade;
            weaponUnlockWeight = weaponUnlock;
            shipOffenseWeight = shipOffense;
            shipDefenseWeight = shipDefense;
            shipEconomyWeight = shipEconomy;
        }

        public AutoBuyStrategy Strategy => strategy;
        public string DisplayName => displayName;
        public string Description => description;

        public float GetWeight(AutoBuyCategory category)
        {
            switch (category)
            {
                case AutoBuyCategory.WeaponUpgrade: return weaponUpgradeWeight;
                case AutoBuyCategory.WeaponUnlock: return weaponUnlockWeight;
                case AutoBuyCategory.ShipOffense: return shipOffenseWeight;
                case AutoBuyCategory.ShipDefense: return shipDefenseWeight;
                case AutoBuyCategory.ShipEconomy: return shipEconomyWeight;
                default: return 0f;
            }
        }
    }
}
