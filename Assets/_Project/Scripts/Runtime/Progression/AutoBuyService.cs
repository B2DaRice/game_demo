using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Persistence;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Auto mode: when enabled, periodically spends the player's credits on upgrades and weapon unlocks using
    /// the active strategy's AutoBuyProfile (see AutoBuyPlanner for the rule). If the top-priority purchase isn't
    /// affordable yet it waits and saves up, rather than buying something lower priority.
    /// Purchases go through the same UpgradeService / WeaponInventory calls as the UI, so events, saves and
    /// stats behave exactly as if the player tapped the button.
    /// </summary>
    public class AutoBuyService
    {
        private enum ActionKind
        {
            ShipTrack,
            WeaponTrack,
            WeaponUnlock,
        }

        private readonly struct PurchaseAction
        {
            public PurchaseAction(ActionKind kind, string ownerId, UpgradeTrack track, WeaponDefinition weapon)
            {
                Kind = kind;
                OwnerId = ownerId;
                Track = track;
                Weapon = weapon;
            }

            public ActionKind Kind { get; }
            public string OwnerId { get; }
            public UpgradeTrack Track { get; }
            public WeaponDefinition Weapon { get; }
        }

        private readonly ContentDatabase content;
        private readonly SaveData save;
        private readonly CurrencyWallet wallet;
        private readonly UpgradeService upgrades;
        private readonly WeaponInventory weapons;
        private readonly List<AutoBuyCandidate> candidates = new List<AutoBuyCandidate>(32);
        private readonly List<PurchaseAction> actions = new List<PurchaseAction>(32);
        private float timer;

        public AutoBuyService(ContentDatabase content, SaveData save, CurrencyWallet wallet, UpgradeService upgrades, WeaponInventory weapons)
        {
            this.content = content;
            this.save = save;
            this.wallet = wallet;
            this.upgrades = upgrades;
            this.weapons = weapons;
        }

        public bool Enabled => save.autoBuyEnabled;

        public AutoBuyStrategy Strategy => (AutoBuyStrategy)save.autoBuyStrategy;

        public AutoBuyProfile ActiveProfile => content.AutoBuy != null ? content.AutoBuy.GetProfile(Strategy) : null;

        public void SetEnabled(bool enabled)
        {
            save.autoBuyEnabled = enabled;
            timer = 0f; // shop immediately when switched on
        }

        public void SetStrategy(AutoBuyStrategy strategy)
        {
            save.autoBuyStrategy = (int)strategy;
            timer = 0f;
        }

        /// <summary>Call every frame with real (unscaled) delta time.</summary>
        public void Tick(float unscaledDeltaTime)
        {
            if (!Enabled || content.AutoBuy == null) return;

            timer -= unscaledDeltaTime;
            if (timer > 0f) return;
            timer = content.AutoBuy.TickIntervalSeconds;
            RunPurchases();
        }

        /// <summary>Buys in priority order until the top pick is unaffordable. Returns the number bought.</summary>
        public int RunPurchases()
        {
            AutoBuyProfile profile = ActiveProfile;
            if (profile == null) return 0;

            int bought = 0;
            int limit = content.AutoBuy.MaxPurchasesPerTick;
            while (bought < limit)
            {
                GatherCandidates();
                int pick = AutoBuyPlanner.PickBest(candidates, profile);
                if (pick < 0 || !wallet.CanAfford(candidates[pick].Cost)) break; // save up for the top priority
                if (!Execute(actions[pick])) break;
                bought++;
            }
            return bought;
        }

        private void GatherCandidates()
        {
            candidates.Clear();
            actions.Clear();

            ShipDefinition ship = content.Ship;
            foreach (UpgradeTrack track in ship.Upgrades)
            {
                if (upgrades.IsMaxed(ship.Id, track)) continue;
                Add(AutoBuyPlanner.CategoryFor(track.Stat, true), upgrades.GetCost(ship.Id, track), new PurchaseAction(ActionKind.ShipTrack, ship.Id, track, null));
            }

            bool unlockOffered = false;
            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (!weapons.IsOwned(weapon))
                {
                    // Every unlock costs the same, so offer only the first available one (in tab order).
                    if (!unlockOffered && weapons.IsAvailable(weapon))
                    {
                        Add(AutoBuyCategory.WeaponUnlock, weapons.GetUnlockCost(), new PurchaseAction(ActionKind.WeaponUnlock, weapon.Id, null, weapon));
                        unlockOffered = true;
                    }
                    continue;
                }

                foreach (UpgradeTrack track in weapon.Upgrades)
                {
                    if (upgrades.IsMaxed(weapon.Id, track)) continue;
                    Add(AutoBuyCategory.WeaponUpgrade, upgrades.GetCost(weapon.Id, track), new PurchaseAction(ActionKind.WeaponTrack, weapon.Id, track, weapon));
                }
            }
        }

        private void Add(AutoBuyCategory category, double cost, PurchaseAction action)
        {
            candidates.Add(new AutoBuyCandidate(category, cost));
            actions.Add(action);
        }

        private bool Execute(PurchaseAction action)
        {
            switch (action.Kind)
            {
                case ActionKind.WeaponUnlock: return weapons.TryUnlock(action.Weapon);
                default: return upgrades.TryPurchase(action.OwnerId, action.Track);
            }
        }
    }
}
