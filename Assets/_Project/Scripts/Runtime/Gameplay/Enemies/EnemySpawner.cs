using AstroIdle.Content;
using AstroIdle.Events;
using UnityEngine;
using UnityEngine.Pool;

namespace AstroIdle.Gameplay.Enemies
{
    /// <summary>Pools and spawns enemies. The only way enemies enter or leave play.</summary>
    public class EnemySpawner
    {
        private readonly Transform root;
        private readonly ObjectPool<Enemy> pool;
        private GameplayContext context;

        public EnemySpawner(Transform root)
        {
            this.root = root;
            pool = new ObjectPool<Enemy>(
                Create,
                e => e.gameObject.SetActive(true),
                e => e.gameObject.SetActive(false),
                e => Object.Destroy(e.gameObject),
                collectionCheck: true,
                defaultCapacity: 32,
                maxSize: 256);
        }

        /// <summary>Must be called once after the GameplayContext exists.</summary>
        public void Bind(GameplayContext gameplay)
        {
            context = gameplay;
        }

        public Enemy Spawn(EnemyDefinition definition, Vector2 position, EnemyScaling scaling)
        {
            Enemy enemy = pool.Get();
            enemy.Activate(definition, scaling, position, context);
            GameEvents.RaiseEnemySpawned();
            return enemy;
        }

        /// <summary>Called by Enemy.Despawn(); don't call directly.</summary>
        public void Release(Enemy enemy)
        {
            pool.Release(enemy);
        }

        /// <summary>Removes every live enemy without rewards (used on wave failure / debug skip).</summary>
        public void DespawnAll()
        {
            var enemies = context.Registry.Enemies;
            // Iterate backwards: each Despawn unregisters the current element.
            for (int i = enemies.Count - 1; i >= 0; i--) enemies[i].Despawn();
        }

        private Enemy Create()
        {
            var go = new GameObject("Enemy");
            go.transform.SetParent(root, false);
            go.AddComponent<SpriteRenderer>();
            return go.AddComponent<Enemy>();
        }
    }
}
