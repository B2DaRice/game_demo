using System;
using AstroIdle.Content;
using AstroIdle.Progression;
using AstroIdle.Utilities;

namespace AstroIdle.UI.Bindings
{
    /// <summary>Row for one upgrade track: level, current -> next value, and a buy button showing the cost.</summary>
    public class UpgradeRowBinding : IRowBinding
    {
        private readonly UpgradeService upgrades;
        private readonly string ownerId;
        private readonly UpgradeTrack track;
        private readonly Func<UpgradeTrack, float> evaluate;

        /// <param name="evaluate">Returns the final stat value; called with null for current, or the track to preview +1 level.</param>
        public UpgradeRowBinding(UpgradeService upgrades, string ownerId, UpgradeTrack track, Func<UpgradeTrack, float> evaluate)
        {
            this.upgrades = upgrades;
            this.ownerId = ownerId;
            this.track = track;
            this.evaluate = evaluate;
        }

        public string Title
        {
            get
            {
                int level = upgrades.GetLevel(ownerId, track);
                return track.MaxLevel > 0
                    ? $"{track.DisplayName}  <size=28>Lv {level}/{track.MaxLevel}</size>"
                    : $"{track.DisplayName}  <size=28>Lv {level}</size>";
            }
        }

        public string Detail
        {
            get
            {
                string current = StatFormatter.Format(track.Stat, evaluate(null));
                if (upgrades.IsMaxed(ownerId, track)) return $"{track.Description}\n{current}  (max)";
                string next = StatFormatter.Format(track.Stat, evaluate(track));
                return $"{track.Description}\n{current}  ->  <color=#7CFF9A>{next}</color>";
            }
        }

        public bool HasButton => true;

        public string ButtonLabel => upgrades.IsMaxed(ownerId, track) ? "MAX" : NumberFormatter.Format(upgrades.GetCost(ownerId, track));

        public bool ButtonEnabled => upgrades.CanPurchase(ownerId, track);

        public RowStyle Style => RowStyle.Normal;

        public void OnClick()
        {
            upgrades.TryPurchase(ownerId, track);
        }
    }
}
