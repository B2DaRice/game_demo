using System.Collections.Generic;
using AstroIdle.Content;
using UnityEngine;

namespace AstroIdle.Gameplay.Weapons
{
    /// <summary>Holds and ticks every weapon the player owns. All owned weapons fire simultaneously.</summary>
    public class WeaponSystem
    {
        private readonly GameplayContext context;
        private readonly List<WeaponRuntime> mounts = new List<WeaponRuntime>();

        public WeaponSystem(GameplayContext context)
        {
            this.context = context;
        }

        public IReadOnlyList<WeaponRuntime> Mounts => mounts;

        /// <summary>Adds a mount for every owned weapon that doesn't have one yet.</summary>
        public void SyncWithInventory()
        {
            foreach (WeaponDefinition weapon in context.Weapons.OwnedWeapons())
            {
                if (!HasMount(weapon))
                {
                    var mount = new WeaponRuntime(weapon);
                    mount.RefreshStats(context.Upgrades);
                    mounts.Add(mount);
                }
            }
        }

        public void RefreshStats()
        {
            for (int i = 0; i < mounts.Count; i++) mounts[i].RefreshStats(context.Upgrades);
        }

        public void Tick(float deltaTime, Vector2 origin)
        {
            for (int i = 0; i < mounts.Count; i++) mounts[i].Tick(deltaTime, origin, context);
        }

        private bool HasMount(WeaponDefinition weapon)
        {
            for (int i = 0; i < mounts.Count; i++)
            {
                if (mounts[i].Definition == weapon) return true;
            }
            return false;
        }
    }
}
