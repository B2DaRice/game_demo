using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Persistence;
using UnityEngine;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Tracks which weapons are available (can be unlocked) and owned (equipped and firing).
    /// A new game starts with one random weapon. Every other available weapon can be unlocked for the same
    /// price (EconomyConfig.weaponUnlockCost, optionally growing per weapon owned).
    /// Weapons that aren't AvailableFromStart become available through a milestone (<see cref="MakeAvailable"/>).
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

        public bool IsAvailable(WeaponDefinition weapon) => save.unlockedWeapons.Contains(weapon.Id);

        public bool IsOwned(WeaponDefinition weapon) => save.ownedWeapons.Contains(weapon.Id);

        public int OwnedCount => save.ownedWeapons.Count;

        /// <summary>The weapon this save started with, or the first owned weapon for older saves.</summary>
        public WeaponDefinition StartingWeapon
        {
            get
            {
                WeaponDefinition starter = content.GetWeapon(save.startingWeaponId);
                if (starter != null) return starter;
                foreach (WeaponDefinition weapon in OwnedWeapons()) return weapon;
                return null;
            }
        }

        public IEnumerable<WeaponDefinition> OwnedWeapons()
        {
            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (IsOwned(weapon)) yield return weapon;
            }
        }

        /// <summary>Current price to unlock any weapon.</summary>
        public double GetUnlockCost()
        {
            EconomyConfig economy = content.Economy;
            return ProgressionMath.WeaponUnlockCost(economy.WeaponUnlockCost, economy.WeaponUnlockCostGrowth, OwnedCount);
        }

        public bool CanUnlock(WeaponDefinition weapon)
        {
            return IsAvailable(weapon) && !IsOwned(weapon) && wallet.CanAfford(GetUnlockCost());
        }

        /// <summary>
        /// Runs on every load: marks AvailableFromStart weapons as available, and if the save owns no weapon
        /// (a new game), grants a random starting weapon chosen with <paramref name="roll01"/> (0..1).
        /// Safe to repeat, so weapons added in later content updates reach existing saves too.
        /// </summary>
        public void EnsureStartingWeapons(float roll01)
        {
            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (weapon.AvailableFromStart && !IsAvailable(weapon)) save.unlockedWeapons.Add(weapon.Id);
            }

            if (save.ownedWeapons.Count > 0) return;

            var candidates = new List<WeaponDefinition>();
            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (weapon.CanBeStartingWeapon) candidates.Add(weapon);
            }

            int index = ProgressionMath.PickIndex(candidates.Count, roll01);
            if (index < 0) return;

            WeaponDefinition starter = candidates[index];
            save.startingWeaponId = starter.Id;
            if (!IsAvailable(starter)) save.unlockedWeapons.Add(starter.Id);
            save.ownedWeapons.Add(starter.Id);
        }

        /// <summary>Makes a gated weapon unlockable (used by milestone rewards).</summary>
        public void MakeAvailable(string weaponId)
        {
            WeaponDefinition weapon = content.GetWeapon(weaponId);
            if (weapon == null)
            {
                Debug.LogWarning($"[WeaponInventory] Tried to make unknown weapon '{weaponId}' available.");
                return;
            }

            if (IsAvailable(weapon)) return;
            save.unlockedWeapons.Add(weapon.Id);
            GameEvents.RaiseWeaponUnlocked(weapon);
        }

        /// <summary>Spends the unlock cost and equips the weapon.</summary>
        public bool TryUnlock(WeaponDefinition weapon)
        {
            if (!IsAvailable(weapon) || IsOwned(weapon)) return false;
            if (!wallet.TrySpend(GetUnlockCost())) return false;

            save.ownedWeapons.Add(weapon.Id);
            GameEvents.RaiseWeaponPurchased(weapon);
            return true;
        }
    }
}
