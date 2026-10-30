using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.Utilities;

namespace AstroIdle.UI.Bindings
{
    /// <summary>
    /// Header row for the selected weapon: description and current stats (base stats when not yet owned),
    /// plus an UNLOCK button while it can be unlocked, or the milestone hint while it's still gated.
    /// </summary>
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
        private bool Available => context.Weapons.IsAvailable(weapon);

        public string Title
        {
            get
            {
                if (Owned) return $"<b>{weapon.DisplayName}</b>  <size=28><color=#7CFF9A>EQUIPPED</color></size>";
                if (Available) return $"<b>{weapon.DisplayName}</b>  <size=28><color=#FFD95A>NOT UNLOCKED</color></size>";
                return $"<b>{weapon.DisplayName}</b>  <size=28>LOCKED</size>";
            }
        }

        public string Detail
        {
            get
            {
                string stats = StatsLine();
                if (Owned) return $"{weapon.Description}\n{stats}";
                if (Available) return $"{weapon.Description}\n{stats}";
                return $"{weapon.Description}\n{stats}\nAvailable after: {unlockHint}";
            }
        }

        public bool HasButton => Available && !Owned;

        public string ButtonLabel => "UNLOCK\n" + NumberFormatter.Format(context.Weapons.GetUnlockCost());

        public bool ButtonEnabled => context.Weapons.CanUnlock(weapon);

        public RowStyle Style => Owned ? RowStyle.Header : RowStyle.Locked;

        public void OnClick()
        {
            context.Weapons.TryUnlock(weapon);
        }

        private string StatsLine()
        {
            var upgrades = context.Upgrades;
            string damage = StatFormatter.Format(StatId.Damage, upgrades.GetWeaponStat(weapon, StatId.Damage));
            string rate = StatFormatter.Format(StatId.FireRate, upgrades.GetWeaponStat(weapon, StatId.FireRate));
            string count = StatFormatter.Format(StatId.ProjectileCount, upgrades.GetWeaponStat(weapon, StatId.ProjectileCount));
            string range = StatFormatter.Format(StatId.Range, upgrades.GetWeaponStat(weapon, StatId.Range));
            return $"DMG {damage}  |  {rate}  |  x{count}  |  RNG {range}";
        }

        private static string FindUnlockHint(ContentDatabase content, WeaponDefinition weapon)
        {
            foreach (MilestoneDefinition milestone in content.Milestones)
            {
                if (milestone.UnlockWeaponId == weapon.Id) return milestone.Description;
            }
            return "not yet available.";
        }
    }
}
