using System;
using AstroIdle.Content;
using AstroIdle.Gameplay;
using AstroIdle.Gameplay.Player;
using AstroIdle.Gameplay.Waves;
using AstroIdle.Persistence;
using AstroIdle.Progression;

namespace AstroIdle.Core
{
    /// <summary>
    /// Every service and top-level object for the running game, assembled by GameBootstrap (the composition
    /// root) and handed to the UI. Think of it as the game's dependency container — but explicit: there is
    /// no global lookup, so dependencies stay visible in constructors / Initialize methods.
    /// </summary>
    public class GameContext
    {
        public ContentDatabase Content { get; set; }
        public SaveData Save { get; set; }
        public SaveService SaveService { get; set; }
        public CurrencyWallet Wallet { get; set; }
        public UpgradeService Upgrades { get; set; }
        public WeaponInventory Weapons { get; set; }
        public MilestoneService Milestones { get; set; }
        public RewardService Rewards { get; set; }
        public AutoBuyService AutoBuy { get; set; }
        public GameplayContext Gameplay { get; set; }
        public PlayerShip Player { get; set; }
        public WaveDirector Waves { get; set; }

        /// <summary>Writes the save file immediately.</summary>
        public Action SaveNow { get; set; }

        /// <summary>Deletes the save and restarts the scene (debug tools).</summary>
        public Action ResetProgress { get; set; }
    }
}
