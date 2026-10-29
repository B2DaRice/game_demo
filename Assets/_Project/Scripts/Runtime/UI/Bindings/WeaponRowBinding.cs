using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.Utilities;

namespace AstroIdle.UI.Bindings
{
    /// <summary>Header row for a weapon: locked / purchasable / owned state, summary stats, and buy button.</summary>
    public class WeaponRowBinding : IRowBinding
    {
        private readonly GameContext context;
        private readonly WeaponDefinition weapon;
        private readonly string unlockHint;

        public WeaponRowBinding(GameContext context, WeaponDefinition weapon)
        {
            this.context = context;
            this.weapon = weapon;
            unlockHint = FindUnlockHint(context.Content, weapon);
        }

        private bool Owned => context.Weapons.IsOwned(weapon);
        private bool Unlocked => context.Weapons.IsUnlocked(weapon);

        public string Title
        {
            get
            {
                if (Owned) return $"<b>{weapon.DisplayName}</b>  <size=28><color=#7CFF9A>EQUIPPED</color></size>";
                if (Unlocked) return $"<b>{weapon.DisplayName}</b>  <size=28><color=#FFD95A>AVAILABLE</color></size>";
                return $"<b>{weapon.DisplayName}</b>  <size=28>LOCKED</size>";
            }
        }

        public string Detail
        {
            get
            {
                if (!Owned) return Unlocked ? $"{weapon.Description}\nBuy to equip. It fires alongside your other weapons." : $"{weapon.Description}\nUnlock: {unlockHint}";

                var upgrades = context.Upgrades;
                string damage = StatFormatter.Format(StatId.Damage, upgrades.GetWeaponStat(weapon, StatId.Damage));
                string rate = StatFormatter.Format(StatId.FireRate, upgrades.GetWeaponStat(weapon, StatId.FireRate));
                string count = StatFormatter.Format(StatId.ProjectileCount, upgrades.GetWeaponStat(weapon, StatId.ProjectileCount));
                return $"{weapon.Description}\nDMG {damage}  |  {rate}  |  x{count} per volley";
            }
        }

        public bool HasButton => Unlocked && !Owned;

        public string ButtonLabel => NumberFormatter.Format(weapon.PurchaseCost);

        public bool ButtonEnabled => context.Wallet.CanAfford(weapon.PurchaseCost);

        public RowStyle Style => Unlocked ? RowStyle.Header : RowStyle.Locked;

        public void OnClick()
        {
            context.Weapons.TryPurchase(weapon);
        }

        private static string FindUnlockHint(ContentDatabase content, WeaponDefinition weapon)
        {
            foreach (MilestoneDefinition milestone in content.Milestones)
            {
                if (milestone.UnlockWeaponId == weapon.Id) return milestone.Description;
            }
            return "Not yet available.";
        }
    }
}
