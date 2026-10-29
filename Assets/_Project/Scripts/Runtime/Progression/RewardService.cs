using System;
using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Persistence;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Turns enemy kills into credits (applying the ship's credit multiplier), counts kills, and maintains a
    /// smoothed "credits per second" estimate that drives the HUD and offline earnings.
    /// </summary>
    public class RewardService : IDisposable
    {
        private const float SampleSeconds = 1f;

        private readonly SaveData save;
        private readonly CurrencyWallet wallet;
        private readonly UpgradeService upgrades;
        private readonly EconomyConfig economy;

        private double sampleCredits;
        private float sampleTime;

        public RewardService(SaveData save, CurrencyWallet wallet, UpgradeService upgrades, EconomyConfig economy)
        {
            this.save = save;
            this.wallet = wallet;
            this.upgrades = upgrades;
            this.economy = economy;
            GameEvents.EnemyKilled += OnEnemyKilled;
        }

        /// <summary>Smoothed live income, persisted in the save for offline earnings.</summary>
        public double CreditsPerSecond => save.creditsPerSecond;

        /// <summary>Call every frame with game (scaled) delta time.</summary>
        public void Tick(float deltaTime)
        {
            sampleTime += deltaTime;
            if (sampleTime < SampleSeconds) return;

            // Exponential moving average: each sample moves the estimate a fraction toward the latest rate.
            double instantRate = sampleCredits / sampleTime;
            double blend = Math.Min(1d, sampleTime / Math.Max(1f, economy.CreditRateWindowSeconds));
            save.creditsPerSecond += (instantRate - save.creditsPerSecond) * blend;

            sampleCredits = 0d;
            sampleTime = 0f;
        }

        public void Dispose()
        {
            GameEvents.EnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            save.totalKills++;
            double amount = info.CreditReward * upgrades.GetShipStat(StatId.CreditMultiplier);
            wallet.Add(amount);
            sampleCredits += amount;
        }
    }
}
