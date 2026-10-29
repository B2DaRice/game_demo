using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.UI.Bindings;

namespace AstroIdle.UI.Tabs
{
    /// <summary>Every weapon (locked, purchasable or owned); owned weapons list their upgrade tracks below.</summary>
    public class WeaponsTab : IPanelTab
    {
        private readonly GameContext context;

        public WeaponsTab(GameContext context)
        {
            this.context = context;
        }

        public string Title => "WEAPONS";

        public IEnumerable<IRowBinding> BuildRows()
        {
            foreach (WeaponDefinition weapon in context.Content.Weapons)
            {
                yield return new WeaponRowBinding(context, weapon);
                if (!context.Weapons.IsOwned(weapon)) continue;

                foreach (UpgradeTrack track in weapon.Upgrades)
                {
                    yield return new UpgradeRowBinding(context.Upgrades, weapon.Id, track, preview => context.Upgrades.GetWeaponStat(weapon, track.Stat, preview));
                }
            }
        }
    }
}
