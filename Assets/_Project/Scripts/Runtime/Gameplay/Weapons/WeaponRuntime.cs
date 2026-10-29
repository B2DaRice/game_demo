using AstroIdle.Content;
using AstroIdle.Gameplay.Combat;
using AstroIdle.Gameplay.Enemies;
using AstroIdle.Progression;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Weapons
{
    /// <summary>
    /// One equipped weapon: caches its upgraded stats, counts down its cooldown, picks the nearest target in
    /// range and fires a volley (with simple target leading for straight projectiles).
    /// Note: fires at most one volley per frame, so fire rates above ~60/s are effectively capped.
    /// </summary>
    public class WeaponRuntime
    {
        private float cooldown;
        private float range;
        private float fireRate;
        private ProjectileSpec spec;
        private int projectileCount;

        public WeaponRuntime(WeaponDefinition definition)
        {
            Definition = definition;
            cooldown = Random.Range(0f, 0.3f); // desync weapons so they don't all fire on the same frame
        }

        public WeaponDefinition Definition { get; }

        public void RefreshStats(UpgradeService upgrades)
        {
            range = upgrades.GetWeaponStat(Definition, StatId.Range);
            fireRate = Mathf.Max(0.01f, upgrades.GetWeaponStat(Definition, StatId.FireRate));
            projectileCount = Mathf.Max(1, Mathf.RoundToInt(upgrades.GetWeaponStat(Definition, StatId.ProjectileCount)));

            spec = new ProjectileSpec
            {
                Damage = upgrades.GetWeaponStat(Definition, StatId.Damage),
                Speed = upgrades.GetWeaponStat(Definition, StatId.ProjectileSpeed),
                Lifetime = Definition.ProjectileLifetime,
                Pierce = Mathf.Max(0, Mathf.RoundToInt(upgrades.GetWeaponStat(Definition, StatId.Pierce))),
                SplashRadius = upgrades.GetWeaponStat(Definition, StatId.SplashRadius),
                Motion = Definition.Motion,
                TurnRate = Definition.HomingTurnRate,
                Shape = Definition.ProjectileShape,
                Size = Definition.ProjectileSize,
                Color = Definition.Color,
            };
        }

        public void Tick(float deltaTime, Vector2 origin, GameplayContext context)
        {
            cooldown -= deltaTime;
            if (cooldown > 0f) return;

            Enemy target = context.Registry.FindNearest(origin, range);
            if (target == null)
            {
                cooldown = 0f; // stay ready to fire the moment something comes into range
                return;
            }

            Fire(origin, target, context);
            cooldown += 1f / fireRate;
        }

        private void Fire(Vector2 origin, Enemy target, GameplayContext context)
        {
            Vector2 aimPoint = target.Position;
            if (spec.Motion == ProjectileMotion.Straight && spec.Speed > 0f)
            {
                float travelTime = Vector2.Distance(origin, target.Position) / spec.Speed;
                aimPoint += target.Velocity * travelTime;
            }

            Vector2 aim = aimPoint - origin;
            if (aim.sqrMagnitude < 0.0001f) aim = Vector2.up;
            aim.Normalize();

            for (int i = 0; i < projectileCount; i++)
            {
                Vector2 direction = VectorUtils.Rotate(aim, VectorUtils.SpreadOffset(i, projectileCount, Definition.SpreadAngle));
                context.Projectiles.FirePlayer(spec, origin, direction, target);
            }
        }
    }
}
