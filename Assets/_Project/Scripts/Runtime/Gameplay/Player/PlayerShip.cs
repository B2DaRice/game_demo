using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Gameplay.Enemies;
using AstroIdle.Gameplay.Weapons;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Player
{
    /// <summary>
    /// The player's ship: hull/regen, autopilot movement, facing, and its weapons. Stats come from
    /// UpgradeService and are refreshed whenever an upgrade or weapon is bought.
    /// When the hull reaches zero it raises PlayerDestroyed and waits for WaveDirector to call Respawn().
    /// </summary>
    public class PlayerShip : MonoBehaviour
    {
        private const float TurnSpeedDegrees = 540f;
        private const float HitFlashSeconds = 0.1f;
        private const float InvulnerableBlinkRate = 12f;

        private ShipDefinition definition;
        private GameplayContext context;
        private SpriteRenderer spriteRenderer;
        private ShipAutopilot autopilot;
        private WeaponSystem weapons;
        private float hullRegen;
        private float moveSpeed;
        private float invulnerableTimer;
        private float flashTimer;
        private bool statsDirty;

        public float Hull { get; private set; }
        public float MaxHull { get; private set; }
        public bool IsAlive { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float CollisionRadius => definition.CollisionRadius;

        public void Initialize(ShipDefinition shipDefinition, GameplayContext gameplay)
        {
            definition = shipDefinition;
            context = gameplay;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = ShapeSpriteFactory.Get(definition.Shape);
            spriteRenderer.color = definition.Color;
            spriteRenderer.sortingOrder = 10;
            transform.localScale = Vector3.one * definition.VisualRadius;

            autopilot = new ShipAutopilot();
            weapons = new WeaponSystem(context);
            weapons.SyncWithInventory();

            RefreshStats(true);
            IsAlive = true;
            SetPosition(context.Arena.Center);

            GameEvents.UpgradePurchased += OnUpgradePurchased;
            GameEvents.WeaponPurchased += OnWeaponPurchased;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || invulnerableTimer > 0f || amount <= 0f) return;

            Hull -= amount;
            flashTimer = HitFlashSeconds;
            if (Hull > 0f) return;

            Hull = 0f;
            IsAlive = false;
            spriteRenderer.enabled = false;
            context.Effects.SpawnExplosion(Position, definition.Color, 1.2f);
            GameEvents.RaisePlayerDestroyed();
        }

        /// <summary>Restores full hull at the arena center with temporary invulnerability.</summary>
        public void Respawn()
        {
            RefreshStats(true);
            IsAlive = true;
            Velocity = Vector2.zero;
            invulnerableTimer = definition.RespawnInvulnerabilitySeconds;
            spriteRenderer.enabled = true;
            spriteRenderer.color = definition.Color;
            SetPosition(context.Arena.Center);
        }

        private void OnDestroy()
        {
            GameEvents.UpgradePurchased -= OnUpgradePurchased;
            GameEvents.WeaponPurchased -= OnWeaponPurchased;
        }

        private void Update()
        {
            if (context == null || !IsAlive) return;

            float dt = Time.deltaTime;
            if (statsDirty) RefreshStats(false);

            Hull = Mathf.Min(MaxHull, Hull + hullRegen * dt);
            UpdateMovement(dt);
            UpdateVisuals(dt);
            weapons.Tick(dt, Position);
        }

        private void UpdateMovement(float dt)
        {
            Velocity = autopilot.ComputeVelocity(Position, Velocity, moveSpeed, context.Registry, context.Arena, dt);
            SetPosition(context.Arena.ClampToShipBounds(Position + Velocity * dt));

            // Face the nearest enemy (we're always shooting at something), otherwise face travel direction.
            Enemy nearest = context.Registry.FindNearest(Position, float.MaxValue);
            Vector2 facing = nearest != null ? nearest.Position - Position : Velocity;
            if (facing.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, VectorUtils.FacingRotation(facing), TurnSpeedDegrees * dt);
            }
        }

        private void UpdateVisuals(float dt)
        {
            Color color = definition.Color;
            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                color = Color.Lerp(definition.Color, new Color(1f, 0.3f, 0.3f, 1f), 0.8f);
            }

            if (invulnerableTimer > 0f)
            {
                invulnerableTimer -= dt;
                bool dim = Mathf.Repeat(invulnerableTimer * InvulnerableBlinkRate, 1f) < 0.5f;
                color.a = dim ? 0.35f : 1f;
            }

            spriteRenderer.color = color;
        }

        private void RefreshStats(bool fillHull)
        {
            float previousMax = MaxHull;
            MaxHull = context.Upgrades.GetShipStat(StatId.MaxHull);
            hullRegen = context.Upgrades.GetShipStat(StatId.HullRegen);
            moveSpeed = context.Upgrades.GetShipStat(StatId.MoveSpeed);

            // Upgrading max hull also grants the added hull immediately.
            Hull = fillHull ? MaxHull : Mathf.Min(MaxHull, Hull + Mathf.Max(0f, MaxHull - previousMax));

            weapons.RefreshStats();
            statsDirty = false;
        }

        private void SetPosition(Vector2 position)
        {
            Position = position;
            transform.position = position;
        }

        private void OnUpgradePurchased(string upgradeKey)
        {
            statsDirty = true;
        }

        private void OnWeaponPurchased(WeaponDefinition weapon)
        {
            weapons.SyncWithInventory();
            statsDirty = true;
        }
    }
}
