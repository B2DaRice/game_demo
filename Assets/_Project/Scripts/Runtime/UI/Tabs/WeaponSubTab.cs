using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.UI.Tabs
{
    /// <summary>Weapons tab sidebar entry: weapon name plus its total upgrade level, unlock price, or LOCKED.</summary>
    public class WeaponSubTab : ISubTab
    {
        private readonly GameContext context;
        private readonly WeaponDefinition weapon;

        public WeaponSubTab(GameContext context, WeaponDefinition weapon)
        {
            this.context = context;
            this.weapon = weapon;
        }

        public string Title => weapon.DisplayName;

        public string Subtitle
        {
            get
            {
                if (context.Weapons.IsOwned(weapon)) return "Lv " + context.Upgrades.GetTotalLevels(weapon.Id, weapon.Upgrades);
                if (context.Weapons.IsAvailable(weapon)) return "Unlock " + NumberFormatter.Format(context.Weapons.GetUnlockCost());
                return "LOCKED";
            }
        }

        public Color Accent => weapon.Color;

        public bool Dimmed => !context.Weapons.IsOwned(weapon);
    }
}
