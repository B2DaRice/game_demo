using System.Collections.Generic;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// The player ship: base stats, visuals and the ship-wide upgrade tracks shown on the "Ship" tab.
    /// Exactly one must exist in Resources/Content/Ship.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Ship Definition", fileName = "Ship_New")]
    public class ShipDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "ship";
        [SerializeField] private string displayName = "Sentinel";

        [Header("Visuals")]
        [SerializeField] private ShapeType shape = ShapeType.Ship;
        [SerializeField] private Color color = new Color(0.4f, 0.9f, 1f, 1f);
        [SerializeField] private float visualRadius = 0.45f;
        [SerializeField] private float collisionRadius = 0.35f;

        [Header("Base Stats")]
        [SerializeField] private float maxHull = 100f;
        [Tooltip("Hull points regenerated per second.")]
        [SerializeField] private float hullRegen = 1f;
        [Tooltip("World units per second.")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float creditMultiplier = 1f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float fireRateMultiplier = 1f;
        [SerializeField] private float respawnInvulnerabilitySeconds = 2f;

        [Header("Upgrades")]
        [SerializeField] private List<UpgradeTrack> upgrades = new List<UpgradeTrack>();

        public string Id => id;
        public string DisplayName => displayName;
        public ShapeType Shape => shape;
        public Color Color => color;
        public float VisualRadius => visualRadius;
        public float CollisionRadius => collisionRadius;
        public float RespawnInvulnerabilitySeconds => respawnInvulnerabilitySeconds;
        public IReadOnlyList<UpgradeTrack> Upgrades => upgrades;

        /// <summary>Base (un-upgraded) value for a ship stat.</summary>
        public float GetBaseStat(StatId stat)
        {
            switch (stat)
            {
                case StatId.MaxHull: return maxHull;
                case StatId.HullRegen: return hullRegen;
                case StatId.MoveSpeed: return moveSpeed;
                case StatId.CreditMultiplier: return creditMultiplier;
                case StatId.GlobalDamageMultiplier: return damageMultiplier;
                case StatId.GlobalFireRateMultiplier: return fireRateMultiplier;
                default: return 0f;
            }
        }
    }
}
