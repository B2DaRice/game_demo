using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.UI.Bindings;
using AstroIdle.Utilities;

namespace AstroIdle.UI.Tabs
{
    /// <summary>Ship summary plus one row per ship upgrade track.</summary>
    public class ShipTab : IPanelTab
    {
        private readonly GameContext context;

        public ShipTab(GameContext context)
        {
            this.context = context;
        }

        public string Title => "SHIP";

        public IEnumerable<IRowBinding> BuildRows()
        {
            ShipDefinition ship = context.Content.Ship;
            yield return new ActionRowBinding(
                () => $"<b>{ship.DisplayName}</b>",
                () => $"Hull {NumberFormatter.Format(context.Player.Hull)} / {NumberFormatter.Format(context.Player.MaxHull)}   " +
                      $"Credits {StatFormatter.Format(StatId.CreditMultiplier, context.Upgrades.GetShipStat(StatId.CreditMultiplier))}\n" +
                      $"Lifetime kills {NumberFormatter.Format(context.Save.totalKills)}",
                style: RowStyle.Header);

            foreach (UpgradeTrack track in ship.Upgrades)
            {
                yield return new UpgradeRowBinding(context.Upgrades, ship.Id, track, preview => context.Upgrades.GetShipStat(track.Stat, preview));
            }
        }
    }
}
