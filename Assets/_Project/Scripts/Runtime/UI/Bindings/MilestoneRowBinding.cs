using System;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.Utilities;

namespace AstroIdle.UI.Bindings
{
    /// <summary>Read-only row showing a milestone's progress and rewards.</summary>
    public class MilestoneRowBinding : IRowBinding
    {
        private readonly GameContext context;
        private readonly MilestoneDefinition milestone;
        private readonly string rewardText;

        public MilestoneRowBinding(GameContext context, MilestoneDefinition milestone)
        {
            this.context = context;
            this.milestone = milestone;
            rewardText = BuildRewardText(context.Content, milestone);
        }

        private bool Completed => context.Milestones.IsCompleted(milestone);

        public string Title => Completed
            ? $"{milestone.DisplayName}  <size=28><color=#7CFF9A>COMPLETE</color></size>"
            : milestone.DisplayName;

        public string Detail
        {
            get
            {
                if (Completed) return $"{milestone.Description}\nReward: {rewardText}";
                double current = Math.Min(context.Milestones.GetCurrentValue(milestone), milestone.TargetValue);
                return $"{milestone.Description}\n{NumberFormatter.Format(current)} / {NumberFormatter.Format(milestone.TargetValue)}   Reward: {rewardText}";
            }
        }

        public bool HasButton => false;
        public string ButtonLabel => string.Empty;
        public bool ButtonEnabled => false;
        public RowStyle Style => Completed ? RowStyle.Completed : RowStyle.Normal;

        public void OnClick()
        {
        }

        private static string BuildRewardText(ContentDatabase content, MilestoneDefinition milestone)
        {
            string text = string.Empty;
            if (milestone.RewardCredits > 0d) text = NumberFormatter.Format(milestone.RewardCredits) + " credits";

            WeaponDefinition weapon = content.GetWeapon(milestone.UnlockWeaponId);
            if (weapon != null) text += (text.Length > 0 ? " + " : string.Empty) + "unlock " + weapon.DisplayName;

            return text.Length > 0 ? text : "-";
        }
    }
}
