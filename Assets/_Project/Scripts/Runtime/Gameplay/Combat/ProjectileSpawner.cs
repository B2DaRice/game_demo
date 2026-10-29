using System.Collections.Generic;
using AstroIdle.Gameplay.Enemies;
using UnityEngine;
using UnityEngine.Pool;

namespace AstroIdle.Gameplay.Combat
{
    /// <summary>Pools and launches projectiles for both teams.</summary>
    public class ProjectileSpawner
    {
        private readonly Transform root;
        private readonly ObjectPool<Projectile> pool;
        private readonly List<Projectile> active = new List<Projectile>(256);
        private GameplayContext context;

        public ProjectileSpawner(Transform root)
        {
            this.root = root;
            pool = new ObjectPool<Projectile>(
                Create,
                p => p.gameObject.SetActive(true),
                p => p.gameObject.SetActive(false),
                p => Object.Destroy(p.gameObject),
                collectionCheck: true,
                defaultCapacity: 64,
                maxSize: 512);
        }

        public int ActiveCount => active.Count;

        /// <summary>Must be called once after the GameplayContext exists.</summary>
        public void Bind(GameplayContext gameplay)
        {
            context = gameplay;
        }

        public void FirePlayer(in ProjectileSpec spec, Vector2 origin, Vector2 direction, Enemy target)
        {
            Projectile projectile = pool.Get();
            active.Add(projectile);
            projectile.LaunchPlayer(spec, origin, direction, target, context);
        }

        public void FireEnemy(Vector2 origin, Vector2 direction, float speed, float damage, Color color, float size)
        {
            Projectile projectile = pool.Get();
            active.Add(projectile);
            projectile.LaunchEnemy(origin, direction, speed, damage, color, size, context);
        }

        /// <summary>Called by Projectile.Despawn(); don't call directly.</summary>
        public void Release(Projectile projectile)
        {
            active.Remove(projectile);
            pool.Release(projectile);
        }

        public void DespawnAll()
        {
            // Iterate backwards: each Despawn removes the current element.
            for (int i = active.Count - 1; i >= 0; i--) active[i].Despawn();
        }

        private Projectile Create()
        {
            var go = new GameObject("Projectile");
            go.transform.SetParent(root, false);
            go.AddComponent<SpriteRenderer>();
            return go.AddComponent<Projectile>();
        }
    }
}
