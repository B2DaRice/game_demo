using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.UI.Bindings;

namespace AstroIdle.UI.Tabs
{
    /// <summary>
    /// One sub-tab per weapon (left list). The selected weapon shows a header row (stats, plus an Unlock button
    /// when not owned) followed by its upgrade tracks once owned. Opens on the save's starting weapon.
    /// </summary>
    public class WeaponsTab : IPanelTab
    {
        private readonly GameContext context;
        private readonly List<ISubTab> subTabs = new List<ISubTab>();

        public WeaponsTab(GameContext context)
        {
            this.context = context;
            foreach (WeaponDefinition weapon in context.Content.Weapons) subTabs.Add(new WeaponSubTab(context, weapon));
        }

        public string Title => "WEAPONS";

        public IReadOnlyList<ISubTab> SubTabs => subTabs;

        public int DefaultSubTabIndex
        {
            get
            {
                WeaponDefinition starter = context.Weapons.StartingWeapon;
                var weapons = context.Content.Weapons;
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] == starter) return i;
                }
                return 0;
            }
        }

        public IEnumerable<IRowBinding> BuildRows(int subTabIndex)
        {
            var weapons = context.Content.Weapons;
            if (subTabIndex < 0 || subTabIndex >= weapons.Count) yield break;

            WeaponDefinition weapon = weapons[subTabIndex];
            yield return new WeaponRowBinding(context, weapon);
            if (!context.Weapons.IsOwned(weapon)) yield break;

            foreach (UpgradeTrack track in weapon.Upgrades)
            {
                yield return new UpgradeRowBinding(context.Upgrades, weapon.Id, track, preview => context.Upgrades.GetWeaponStat(weapon, track.Stat, preview));
            }
        }
    }
}
