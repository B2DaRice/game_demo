using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// Base (wave 1) stats and behaviour for one enemy type. Health, damage, reward and speed are scaled per
    /// wave by WaveConfig. Which waves an enemy appears in is controlled by WaveConfig's spawn table.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Enemy Definition", fileName = "Enemy_New")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique id referenced by WaveConfig and split settings.")]
        [SerializeField] private string id = "new_enemy";
        [SerializeField] private string displayName = "New Enemy";

        [Header("Visuals")]
        [SerializeField] private ShapeType shape = ShapeType.Rock;
        [SerializeField] private Color color = Color.gray;
        [Tooltip("Visual radius in world units. Collision radius is slightly smaller.")]
        [SerializeField] private float radius = 0.5f;
        [Tooltip("Degrees per second of visual spin. 0 = no spin.")]
        [SerializeField] private float spinSpeed = 0f;
        [Tooltip("Rotate the sprite to face its movement / target (ignored when spinning).")]
        [SerializeField] private bool faceDirection = true;

        [Header("Base Stats (wave 1)")]
        [SerializeField] private float maxHealth = 20f;
        [Tooltip("Damage dealt to the player on collision.")]
        [SerializeField] private float contactDamage = 10f;
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private double creditReward = 3d;

        [Header("Behaviour")]
        [SerializeField] private EnemyBehaviourType behaviour = EnemyBehaviourType.Drift;
        [Tooltip("Bosses survive collisions (damaging the player on a cooldown) and draw on top.")]
        [SerializeField] private bool isBoss = false;

        [Header("Shooter Settings (Shooter behaviour only)")]
        [SerializeField] private float preferredRange = 4.5f;
        [SerializeField] private float fireInterval = 2f;
        [SerializeField] private float projectileDamage = 5f;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private int projectilesPerShot = 1;
        [SerializeField] private float shotSpreadDegrees = 0f;

        [Header("On Death")]
        [Tooltip("Optional enemy id spawned when this one is destroyed by the player (e.g. asteroid -> shards).")]
        [SerializeField] private string splitIntoEnemyId = "";
        [SerializeField] private int splitCount = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public ShapeType Shape => shape;
        public Color Color => color;
        public float Radius => radius;
        public float SpinSpeed => spinSpeed;
        public bool FaceDirection => faceDirection;
        public float MaxHealth => maxHealth;
        public float ContactDamage => contactDamage;
        public float MoveSpeed => moveSpeed;
        public double CreditReward => creditReward;
        public EnemyBehaviourType Behaviour => behaviour;
        public bool IsBoss => isBoss;
        public float PreferredRange => preferredRange;
        public float FireInterval => fireInterval;
        public float ProjectileDamage => projectileDamage;
        public float ProjectileSpeed => projectileSpeed;
        public int ProjectilesPerShot => projectilesPerShot;
        public float ShotSpreadDegrees => shotSpreadDegrees;
        public string SplitIntoEnemyId => splitIntoEnemyId;
        public int SplitCount => splitCount;
    }
}
