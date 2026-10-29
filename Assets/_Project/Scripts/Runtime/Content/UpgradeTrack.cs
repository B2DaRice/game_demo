using System;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// One purchasable, levelable upgrade (e.g. "Bolt Damage"). Tracks are owned by a ShipDefinition or
    /// WeaponDefinition; the player's level is saved under the key "{ownerId}.{trackId}".
    /// Cost of the next level = baseCost * costGrowth ^ currentLevel (see ProgressionMath).
    /// </summary>
    [Serializable]
    public class UpgradeTrack
    {
        [Tooltip("Unique within its owner. Used in save keys, so do not rename after release.")]
        [SerializeField] private string id = "new_upgrade";
        [SerializeField] private string displayName = "New Upgrade";
        [SerializeField] private string description = "";

        [Header("Effect")]
        [SerializeField] private StatId stat = StatId.Damage;
        [SerializeField] private ModifierKind modifier = ModifierKind.Percent;
        [Tooltip("Flat: amount added per level. Percent: 0.1 = +10% per level.")]
        [SerializeField] private float valuePerLevel = 0.1f;

        [Header("Cost")]
        [SerializeField] private double baseCost = 10d;
        [Tooltip("Cost multiplier per level owned. Must be >= 1. Typical idle range: 1.07 - 1.5.")]
        [SerializeField] private float costGrowth = 1.15f;
        [Tooltip("0 = unlimited.")]
        [SerializeField] private int maxLevel = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public StatId Stat => stat;
        public ModifierKind Modifier => modifier;
        public float ValuePerLevel => valuePerLevel;
        public double BaseCost => baseCost;
        public float CostGrowth => costGrowth;
        public int MaxLevel => maxLevel;
    }
}
