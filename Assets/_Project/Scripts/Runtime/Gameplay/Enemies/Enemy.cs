using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Gameplay.Player;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>
    /// A pooled enemy instance. Holds runtime state (health, velocity, timers); movement/attack logic lives
    /// in a stateless <see cref="IEnemyBehaviour"/> chosen by the definition. Only create through EnemySpawner.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        private const float CollisionScale = 0.85f;
        private const float BossContactCooldown = 1f;
        private const float HitFlashSeconds = 0.08f;
        private const float OutOfBoundsDespawnDistance = 8f;

        private SpriteRenderer spriteRenderer;
        private GameplayContext context;
        private IEnemyBehaviour behaviour;
        private EnemyScaling scaling;
        private Color baseColor;
        private float contactCooldown;
        private float flashTimer;

        public EnemyDefinition Definition { get; private set; }
        public bool IsActive { get; private set; }
        public GameplayContext Context => context;

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; set; }

        /// <summary>Direction the sprite should face when the definition has FaceDirection and no spin.</summary>
        public Vector2 Facing { get; set; }

        /// <summary>Collision radius (slightly smaller than the visual radius to feel fair).</summary>
        public float Radius { get; private set; }

        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Speed { get; private set; }
        public float DamageMultiplier { get; private set; }
        public double CreditReward { get; private set; }

        // Behaviour scratch state (behaviours are shared/stateless, so per-enemy state lives here).
        public bool HasEnteredArena { get; set; }
        public float FireTimer { get; set; }
        public float OrbitDirection { get; set; }

        public void Activate(EnemyDefinition definition, EnemyScaling waveScaling, Vector2 position, GameplayContext gameplay)
        {
            Definition = definition;
            scaling = waveScaling;
            context = gameplay;

            MaxHealth = definition.MaxHealth * waveScaling.HealthMultiplier;
            Health = MaxHealth;
            DamageMultiplier = waveScaling.DamageMultiplier;
            Speed = definition.MoveSpeed * waveScaling.SpeedMultiplier;
            CreditReward = definition.CreditReward * waveScaling.RewardMultiplier;
            Radius = definition.Radius * CollisionScale;

            Velocity = Vector2.zero;
            Facing = Vector2.up;
            HasEnteredArena = false;
            FireTimer = 0f;
            OrbitDirection = 1f;
            contactCooldown = 0f;
            flashTimer = 0f;

            baseColor = definition.Color;
            spriteRenderer.sprite = ShapeSpriteFactory.Get(definition.Shape);
            spriteRenderer.color = baseColor;
            spriteRenderer.sortingOrder = definition.IsBoss ? 2 : 0;
            transform.localScale = Vector3.one * definition.Radius;
            transform.rotation = Quaternion.identity;
            SetPosition(position);

            behaviour = EnemyBehaviourFactory.Get(definition.Behaviour);
            IsActive = true;
            context.Registry.Register(this);
            behaviour.OnActivated(this);
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
            transform.position = position;
        }

        public void TakeDamage(float amount)
        {
            if (!IsActive || amount <= 0f) return;
            Health -= amount;
            flashTimer = HitFlashSeconds;
            if (Health <= 0f) Explode(true);
        }

        /// <summary>
        /// Destroys the enemy with an explosion. When <paramref name="killedByPlayer"/> it grants rewards and
        /// splits; when it rammed the player it does neither.
        /// </summary>
        public void Explode(bool killedByPlayer)
        {
            if (!IsActive) return;

            context.Effects.SpawnExplosion(Position, baseColor, Definition.Radius);
            if (killedByPlayer)
            {
                GameEvents.RaiseEnemyKilled(new EnemyKilledInfo(Definition, Position, CreditReward));
                SpawnSplits();
            }
            Despawn();
        }

        /// <summary>Silently removes the enemy (no rewards, no effects). Safe to call more than once.</summary>
        public void Despawn()
        {
            if (!IsActive) return;
            IsActive = false;
            context.Registry.Unregister(this);
            GameEvents.RaiseEnemyRemoved();
            context.Enemies.Release(this);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!IsActive) return;

            float dt = Time.deltaTime;
            behaviour.Tick(this, dt);
            UpdateVisuals(dt);
            CheckPlayerContact(dt);

            if (IsActive && context.Arena.IsFarOutside(Position, OutOfBoundsDespawnDistance)) Despawn();
        }

        private void UpdateVisuals(float dt)
        {
            if (Definition.SpinSpeed != 0f) transform.Rotate(0f, 0f, Definition.SpinSpeed * dt);
            else if (Definition.FaceDirection && Facing.sqrMagnitude > 0.0001f) transform.rotation = VectorUtils.FacingRotation(Facing);

            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                spriteRenderer.color = flashTimer > 0f ? Color.white : baseColor;
            }
        }

        private void CheckPlayerContact(float dt)
        {
            PlayerShip player = context.Player;
            if (contactCooldown > 0f) contactCooldown -= dt;
            if (player == null || !player.IsAlive) return;

            float combined = Radius + player.CollisionRadius;
            if ((player.Position - Position).sqrMagnitude > combined * combined) return;

            float damage = Definition.ContactDamage * DamageMultiplier;
            if (Definition.IsBoss)
            {
                if (contactCooldown > 0f) return;
                contactCooldown = BossContactCooldown;
                player.TakeDamage(damage);
            }
            else
            {
                Explode(false); // remove ourselves first: damage may trigger a wave reset
                player.TakeDamage(damage);
            }
        }

        private void SpawnSplits()
        {
            if (Definition.SplitCount <= 0 || string.IsNullOrEmpty(Definition.SplitIntoEnemyId)) return;

            EnemyDefinition child = context.Content.GetEnemy(Definition.SplitIntoEnemyId);
            if (child == null) return;

            for (int i = 0; i < Definition.SplitCount; i++)
            {
                Vector2 direction = Random.insideUnitCircle.normalized;
                if (direction == Vector2.zero) direction = Vector2.up;
                Enemy spawned = context.Enemies.Spawn(child, Position + direction * 0.2f, scaling);
                spawned.Velocity = direction * spawned.Speed;
                spawned.HasEnteredArena = true;
            }
        }
    }
}
