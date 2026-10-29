using System.Collections.Generic;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// A weapon the ship can own. All owned weapons fire automatically and independently at the nearest
    /// enemy in range. Weapons become purchasable when a MilestoneDefinition unlocks them
    /// (or immediately if <see cref="OwnedAtStart"/>).
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Weapon Definition", fileName = "Weapon_New")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique id. Used in save data, so do not rename after release.")]
        [SerializeField] private string id = "new_weapon";
        [SerializeField] private string displayName = "New Weapon";
        [SerializeField] private string description = "";
        [Tooltip("Order on the Weapons tab (ascending).")]
        [SerializeField] private int sortOrder = 0;

        [Header("Acquisition")]
        [SerializeField] private bool ownedAtStart = false;
        [Tooltip("Credits to buy once unlocked. 0 = granted automatically when unlocked.")]
        [SerializeField] private double purchaseCost = 100d;

        [Header("Base Stats")]
        [SerializeField] private float damage = 10f;
        [Tooltip("Volleys per second.")]
        [SerializeField] private float fireRate = 1f;
        [Tooltip("Projectiles per volley, spread evenly across spreadAngle.")]
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private float spreadAngle = 0f;
        [SerializeField] private float projectileSpeed = 12f;
        [Tooltip("Max targeting distance in world units.")]
        [SerializeField] private float range = 7f;
        [Tooltip("Extra enemies a projectile can pass through.")]
        [SerializeField] private int pierce = 0;
        [Tooltip("0 = single target. Otherwise damages every enemy within this radius on impact.")]
        [SerializeField] private float splashRadius = 0f;

        [Header("Projectile")]
        [SerializeField] private ProjectileMotion motion = ProjectileMotion.Straight;
        [Tooltip("Degrees per second (homing only).")]
        [SerializeField] private float homingTurnRate = 180f;
        [SerializeField] private float projectileLifetime = 1.5f;
        [SerializeField] private ShapeType projectileShape = ShapeType.Bolt;
        [SerializeField] private float projectileSize = 0.12f;
        [SerializeField] private Color color = Color.white;

        [Header("Upgrades")]
        [SerializeField] private List<UpgradeTrack> upgrades = new List<UpgradeTrack>();

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public int SortOrder => sortOrder;
        public bool OwnedAtStart => ownedAtStart;
        public double PurchaseCost => purchaseCost;
        public float SpreadAngle => spreadAngle;
        public ProjectileMotion Motion => motion;
        public float HomingTurnRate => homingTurnRate;
        public float ProjectileLifetime => projectileLifetime;
        public ShapeType ProjectileShape => projectileShape;
        public float ProjectileSize => projectileSize;
        public Color Color => color;
        public IReadOnlyList<UpgradeTrack> Upgrades => upgrades;

        /// <summary>Base (un-upgraded) value for a weapon stat. Use UpgradeService for the final value.</summary>
        public float GetBaseStat(StatId stat)
        {
            switch (stat)
            {
                case StatId.Damage: return damage;
                case StatId.FireRate: return fireRate;
                case StatId.ProjectileCount: return projectileCount;
                case StatId.Pierce: return pierce;
                case StatId.SplashRadius: return splashRadius;
                case StatId.ProjectileSpeed: return projectileSpeed;
                case StatId.Range: return range;
                default: return 0f;
            }
        }
    }
}
