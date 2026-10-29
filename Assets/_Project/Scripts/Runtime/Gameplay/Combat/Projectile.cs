using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Gameplay.Enemies;
using AstroIdle.Gameplay.Player;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Combat
{
    /// <summary>
    /// A pooled projectile for either team. Player projectiles support piercing, splash and homing; enemy
    /// projectiles fly straight at the ship. Spawned and recycled only through <see cref="ProjectileSpawner"/>.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const float HomingAcquireRange = 8f;
        private const float DespawnMargin = 1.5f;

        private static readonly List<Enemy> SplashBuffer = new List<Enemy>(32);

        private readonly HashSet<Enemy> alreadyHit = new HashSet<Enemy>();

        private SpriteRenderer spriteRenderer;
        private GameplayContext context;
        private ProjectileTeam team;
        private ProjectileSpec spec;
        private Vector2 position;
        private Vector2 velocity;
        private float lifetime;
        private int pierceRemaining;
        private Enemy target;

        public bool IsActive { get; private set; }

        public void LaunchPlayer(in ProjectileSpec projectileSpec, Vector2 origin, Vector2 direction, Enemy homingTarget, GameplayContext gameplay)
        {
            spec = projectileSpec;
            target = homingTarget;
            pierceRemaining = spec.Pierce;
            Launch(ProjectileTeam.Player, origin, direction, gameplay);
        }

        public void LaunchEnemy(Vector2 origin, Vector2 direction, float speed, float damage, Color color, float size, GameplayContext gameplay)
        {
            spec = new ProjectileSpec
            {
                Damage = damage,
                Speed = speed,
                Lifetime = 6f,
                Motion = ProjectileMotion.Straight,
                Shape = ShapeType.Dot,
                Size = size,
                Color = color,
            };
            target = null;
            pierceRemaining = 0;
            Launch(ProjectileTeam.Enemy, origin, direction, gameplay);
        }

        /// <summary>Returns the projectile to its pool. Safe to call more than once.</summary>
        public void Despawn()
        {
            if (!IsActive) return;
            IsActive = false;
            target = null;
            context.Projectiles.Release(this);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        private void Launch(ProjectileTeam owner, Vector2 origin, Vector2 direction, GameplayContext gameplay)
        {
            context = gameplay;
            team = owner;
            position = origin;
            velocity = direction.normalized * spec.Speed;
            lifetime = spec.Lifetime;
            alreadyHit.Clear();

            spriteRenderer.sprite = ShapeSpriteFactory.Get(spec.Shape);
            spriteRenderer.color = spec.Color;
            spriteRenderer.sortingOrder = 5;
            transform.localScale = Vector3.one * spec.Size;
            transform.SetPositionAndRotation(position, VectorUtils.FacingRotation(velocity));
            IsActive = true;
        }

        private void Update()
        {
            if (!IsActive) return;

            float dt = Time.deltaTime;
            lifetime -= dt;
            if (lifetime <= 0f)
            {
                Despawn();
                return;
            }

            if (spec.Motion == ProjectileMotion.Homing) SteerTowardTarget(dt);

            Vector2 previous = position;
            position += velocity * dt;
            transform.SetPositionAndRotation(position, VectorUtils.FacingRotation(velocity));

            if (context.Arena.IsFarOutside(position, DespawnMargin))
            {
                Despawn();
                return;
            }

            if (team == ProjectileTeam.Player) CheckEnemyHits(previous);
            else CheckPlayerHit();
        }

        private void SteerTowardTarget(float dt)
        {
            if (target == null || !target.IsActive) target = context.Registry.FindNearest(position, HomingAcquireRange);
            if (target == null) return;

            Vector2 desired = (target.Position - position).normalized * spec.Speed;
            velocity = Vector3.RotateTowards(velocity, desired, spec.TurnRate * Mathf.Deg2Rad * dt, 0f);
        }

        private void CheckEnemyHits(Vector2 previous)
        {
            Enemy hit = context.Registry.FindFirstOverlap(previous, position, spec.Size, alreadyHit, out Vector2 hitPoint);
            if (hit == null) return;

            alreadyHit.Add(hit);
            if (spec.SplashRadius > 0f)
            {
                // Collect first, then damage: kills modify the registry list.
                context.Registry.FindInRadius(hitPoint, spec.SplashRadius, SplashBuffer);
                hit.TakeDamage(spec.Damage);
                for (int i = 0; i < SplashBuffer.Count; i++)
                {
                    if (SplashBuffer[i] != hit) SplashBuffer[i].TakeDamage(spec.Damage);
                }
                context.Effects.SpawnBlast(hitPoint, spec.SplashRadius, spec.Color);
            }
            else
            {
                hit.TakeDamage(spec.Damage);
                context.Effects.SpawnHitSpark(hitPoint, spec.Color);
            }

            pierceRemaining--;
            if (pierceRemaining < 0) Despawn();
        }

        private void CheckPlayerHit()
        {
            PlayerShip player = context.Player;
            if (player == null || !player.IsAlive) return;

            float combined = spec.Size + player.CollisionRadius;
            if ((player.Position - position).sqrMagnitude > combined * combined) return;

            float damage = spec.Damage;
            Vector2 hitPoint = position;
            Despawn(); // despawn before damaging: damage may trigger a wave reset that clears projectiles
            context.Effects.SpawnHitSpark(hitPoint, spec.Color);
            player.TakeDamage(damage);
        }
    }
}
