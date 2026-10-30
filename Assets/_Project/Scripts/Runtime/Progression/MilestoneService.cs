using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Persistence;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Completes milestones when their condition is met and hands out rewards.
    /// Evaluate() is polled a few times per second by the bootstrap rather than reacting to events, which
    /// avoids any dependency on event-handler ordering (e.g. kill counted before the check runs).
    /// </summary>
    public class MilestoneService
    {
        private readonly ContentDatabase content;
        private readonly SaveData save;
        private readonly CurrencyWallet wallet;
        private readonly WeaponInventory weapons;

        public MilestoneService(ContentDatabase content, SaveData save, CurrencyWallet wallet, WeaponInventory weapons)
        {
            this.content = content;
            this.save = save;
            this.wallet = wallet;
            this.weapons = weapons;
        }

        public bool IsCompleted(MilestoneDefinition milestone) => save.completedMilestones.Contains(milestone.Id);

        public double GetCurrentValue(MilestoneDefinition milestone)
        {
            switch (milestone.Condition)
            {
                case MilestoneCondition.ReachWave: return save.highestWave;
                case MilestoneCondition.TotalKills: return save.totalKills;
                case MilestoneCondition.TotalCreditsEarned: return save.totalCreditsEarned;
                default: return 0d;
            }
        }

        public void Evaluate()
        {
            for (int i = 0; i < content.Milestones.Count; i++)
            {
                MilestoneDefinition milestone = content.Milestones[i];
                if (!IsCompleted(milestone) && GetCurrentValue(milestone) >= milestone.TargetValue)
                {
                    Complete(milestone);
                }
            }
        }

        private void Complete(MilestoneDefinition milestone)
        {
            save.completedMilestones.Add(milestone.Id);
            if (milestone.RewardCredits > 0d) wallet.Add(milestone.RewardCredits);
            if (!string.IsNullOrEmpty(milestone.UnlockWeaponId)) weapons.MakeAvailable(milestone.UnlockWeaponId);
            GameEvents.RaiseMilestoneCompleted(milestone);
        }
    }
}
