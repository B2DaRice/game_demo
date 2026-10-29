using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Persistence;
using UnityEngine;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Tracks which weapons are unlocked (purchasable) and owned (equipped and firing).
    /// Lifecycle: locked -> Unlock() (via milestone) -> TryPurchase() -> owned.
    /// </summary>
    public class WeaponInventory
    {
        private readonly ContentDatabase content;
        private readonly SaveData save;
        private readonly CurrencyWallet wallet;

        public WeaponInventory(ContentDatabase content, SaveData save, CurrencyWallet wallet)
        {
            this.content = content;
            this.save = save;
            this.wallet = wallet;
        }

        public bool IsUnlocked(WeaponDefinition weapon) => save.unlockedWeapons.Contains(weapon.Id);

        public bool IsOwned(WeaponDefinition weapon) => save.ownedWeapons.Contains(weapon.Id);

        public IEnumerable<WeaponDefinition> OwnedWeapons()
        {
            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (IsOwned(weapon)) yield return weapon;
            }
        }

        /// <summary>
        /// Grants every "owned at start" weapon. Safe to call on every load, so weapons added to that set in
        /// later content updates reach existing players too.
        /// </summary>
        public void EnsureStartingWeapons()
        {
            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (!weapon.OwnedAtStart) continue;
                if (!IsUnlocked(weapon)) save.unlockedWeapons.Add(weapon.Id);
                if (!IsOwned(weapon)) save.ownedWeapons.Add(weapon.Id);
            }
        }

        /// <summary>Makes a weapon purchasable. Free weapons (cost 0) are granted immediately.</summary>
        public void Unlock(string weaponId)
        {
            WeaponDefinition weapon = content.GetWeapon(weaponId);
            if (weapon == null)
            {
                Debug.LogWarning($"[WeaponInventory] Tried to unlock unknown weapon '{weaponId}'.");
                return;
            }

            if (!IsUnlocked(weapon))
            {
                save.unlockedWeapons.Add(weapon.Id);
                GameEvents.RaiseWeaponUnlocked(weapon);
            }

            if (weapon.PurchaseCost <= 0d && !IsOwned(weapon)) GrantOwnership(weapon);
        }

        public bool TryPurchase(WeaponDefinition weapon)
        {
            if (!IsUnlocked(weapon) || IsOwned(weapon)) return false;
            if (!wallet.TrySpend(weapon.PurchaseCost)) return false;
            GrantOwnership(weapon);
            return true;
        }

        private void GrantOwnership(WeaponDefinition weapon)
        {
            save.ownedWeapons.Add(weapon.Id);
            GameEvents.RaiseWeaponPurchased(weapon);
        }
    }
}
