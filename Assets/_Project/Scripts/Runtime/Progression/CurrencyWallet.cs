using AstroIdle.Events;
using AstroIdle.Persistence;

namespace AstroIdle.Progression
{
    /// <summary>The only class allowed to change the player's credit balance.</summary>
    public class CurrencyWallet
    {
        private readonly SaveData save;

        public CurrencyWallet(SaveData save)
        {
            this.save = save;
        }

        public double Credits => save.credits;

        /// <param name="amount">Credits to add (ignored if &lt;= 0).</param>
        /// <param name="countAsEarned">False for debug grants so they don't complete "earn X" milestones.</param>
        public void Add(double amount, bool countAsEarned = true)
        {
            if (amount <= 0d) return;
            save.credits += amount;
            if (countAsEarned) save.totalCreditsEarned += amount;
            GameEvents.RaiseCreditsChanged(save.credits);
        }

        public bool CanAfford(double cost)
        {
            return save.credits >= cost;
        }

        public bool TrySpend(double cost)
        {
            if (cost < 0d || !CanAfford(cost)) return false;
            save.credits -= cost;
            GameEvents.RaiseCreditsChanged(save.credits);
            return true;
        }
    }
}
