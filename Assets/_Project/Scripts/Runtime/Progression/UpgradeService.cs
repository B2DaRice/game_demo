using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Persistence;

namespace AstroIdle.Progression
{
    /// <summary>
    /// Owns upgrade levels, costs, purchases and final stat evaluation for the ship and weapons.
    /// Gameplay code should ask this service for stats instead of reading definitions directly, so every
    /// modifier (ship upgrades, weapon upgrades, global multipliers) is applied in exactly one place.
    /// </summary>
    public class UpgradeService
    {
        private readonly ContentDatabase content;
        private readonly SaveData save;
        private readonly CurrencyWallet wallet;

        public UpgradeService(ContentDatabase content, SaveData save, CurrencyWallet wallet)
        {
            this.content = content;
            this.save = save;
            this.wallet = wallet;
        }

        public int GetLevel(string ownerId, UpgradeTrack track)
        {
            return save.GetUpgradeLevel(SaveData.UpgradeKey(ownerId, track.Id));
        }

        public bool IsMaxed(string ownerId, UpgradeTrack track)
        {
            return track.MaxLevel > 0 && GetLevel(ownerId, track) >= track.MaxLevel;
        }

        public double GetCost(string ownerId, UpgradeTrack track)
        {
            // Tracks may point at a shared curve in EconomyConfig so equivalent upgrades cost the same everywhere.
            CostCurve curve = content.Economy.GetCostCurve(track.CostCurveId);
            double baseCost = curve != null ? curve.BaseCost : track.BaseCost;
            float growth = curve != null ? curve.CostGrowth : track.CostGrowth;
            return ProgressionMath.UpgradeCost(baseCost, growth, GetLevel(ownerId, track));
        }

        /// <summary>Sum of all upgrade levels bought for one ship/weapon (shown on the weapon sub-tabs).</summary>
        public int GetTotalLevels(string ownerId, IReadOnlyList<UpgradeTrack> tracks)
        {
            int total = 0;
            for (int i = 0; i < tracks.Count; i++) total += GetLevel(ownerId, tracks[i]);
            return total;
        }

        public bool CanPurchase(string ownerId, UpgradeTrack track)
        {
            return !IsMaxed(ownerId, track) && wallet.CanAfford(GetCost(ownerId, track));
        }

        public bool TryPurchase(string ownerId, UpgradeTrack track)
        {
            if (IsMaxed(ownerId, track)) return false;
            if (!wallet.TrySpend(GetCost(ownerId, track))) return false;

            string key = SaveData.UpgradeKey(ownerId, track.Id);
            save.SetUpgradeLevel(key, save.GetUpgradeLevel(key) + 1);
            GameEvents.RaiseUpgradePurchased(key);
            return true;
        }

        /// <summary>Final ship stat. Pass <paramref name="preview"/> to evaluate as if that track had +1 level.</summary>
        public float GetShipStat(StatId stat, UpgradeTrack preview = null)
        {
            ShipDefinition ship = content.Ship;
            return EvaluateStat(ship.Id, ship.Upgrades, stat, ship.GetBaseStat(stat), preview);
        }

        /// <summary>
        /// Final weapon stat including the ship's global damage / fire-rate multipliers.
        /// Pass <paramref name="preview"/> to evaluate as if that track had +1 level.
        /// </summary>
        public float GetWeaponStat(WeaponDefinition weapon, StatId stat, UpgradeTrack preview = null)
        {
            float value = EvaluateStat(weapon.Id, weapon.Upgrades, stat, weapon.GetBaseStat(stat), preview);
            if (stat == StatId.Damage) value *= GetShipStat(StatId.GlobalDamageMultiplier);
            else if (stat == StatId.FireRate) value *= GetShipStat(StatId.GlobalFireRateMultiplier);
            return value;
        }

        private float EvaluateStat(string ownerId, IReadOnlyList<UpgradeTrack> tracks, StatId stat, float baseValue, UpgradeTrack preview)
        {
            float flat = 0f;
            float percent = 0f;
            for (int i = 0; i < tracks.Count; i++)
            {
                UpgradeTrack track = tracks[i];
                if (track.Stat != stat) continue;

                int level = GetLevel(ownerId, track) + (ReferenceEquals(track, preview) ? 1 : 0);
                if (level <= 0) continue;

                float amount = track.ValuePerLevel * level;
                if (track.Modifier == ModifierKind.Flat) flat += amount;
                else percent += amount;
            }
            return ProgressionMath.EvaluateStat(baseValue, flat, percent);
        }
    }
}
