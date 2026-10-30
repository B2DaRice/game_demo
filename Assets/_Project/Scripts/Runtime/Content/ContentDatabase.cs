using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// Read-only registry of all game content. Loaded once at startup from Resources/Content/* so that adding
    /// a new weapon/enemy/milestone is just "create the asset in the right folder" — no code or scene wiring.
    /// </summary>
    public class ContentDatabase
    {
        public const string ShipPath = "Content/Ship";
        public const string WeaponsPath = "Content/Weapons";
        public const string EnemiesPath = "Content/Enemies";
        public const string MilestonesPath = "Content/Milestones";
        public const string ConfigPath = "Content/Config";

        private readonly Dictionary<string, WeaponDefinition> weaponsById;
        private readonly Dictionary<string, EnemyDefinition> enemiesById;

        public ContentDatabase(
            ShipDefinition ship,
            IEnumerable<WeaponDefinition> weapons,
            IEnumerable<EnemyDefinition> enemies,
            IEnumerable<MilestoneDefinition> milestones,
            WaveConfig waves,
            EconomyConfig economy,
            AutoBuyConfig autoBuy)
        {
            Ship = ship;
            Waves = waves;
            Economy = economy;
            AutoBuy = autoBuy;
            Weapons = (weapons ?? Enumerable.Empty<WeaponDefinition>())
                .Where(w => w != null).OrderBy(w => w.SortOrder).ThenBy(w => w.Id).ToList();
            Enemies = (enemies ?? Enumerable.Empty<EnemyDefinition>())
                .Where(e => e != null).OrderBy(e => e.Id).ToList();
            Milestones = (milestones ?? Enumerable.Empty<MilestoneDefinition>())
                .Where(m => m != null).OrderBy(m => m.SortOrder).ThenBy(m => m.Id).ToList();

            weaponsById = BuildLookup(Weapons, w => w.Id);
            enemiesById = BuildLookup(Enemies, e => e.Id);
        }

        public ShipDefinition Ship { get; }
        public IReadOnlyList<WeaponDefinition> Weapons { get; }
        public IReadOnlyList<EnemyDefinition> Enemies { get; }
        public IReadOnlyList<MilestoneDefinition> Milestones { get; }
        public WaveConfig Waves { get; }
        public EconomyConfig Economy { get; }
        public AutoBuyConfig AutoBuy { get; }

        /// <summary>Loads every content asset from the Resources/Content sub-folders.</summary>
        public static ContentDatabase LoadFromResources()
        {
            return new ContentDatabase(
                Resources.LoadAll<ShipDefinition>(ShipPath).FirstOrDefault(),
                Resources.LoadAll<WeaponDefinition>(WeaponsPath),
                Resources.LoadAll<EnemyDefinition>(EnemiesPath),
                Resources.LoadAll<MilestoneDefinition>(MilestonesPath),
                Resources.LoadAll<WaveConfig>(ConfigPath).FirstOrDefault(),
                Resources.LoadAll<EconomyConfig>(ConfigPath).FirstOrDefault(),
                Resources.LoadAll<AutoBuyConfig>(ConfigPath).FirstOrDefault());
        }

        public WeaponDefinition GetWeapon(string id)
        {
            return !string.IsNullOrEmpty(id) && weaponsById.TryGetValue(id, out WeaponDefinition weapon) ? weapon : null;
        }

        public EnemyDefinition GetEnemy(string id)
        {
            return !string.IsNullOrEmpty(id) && enemiesById.TryGetValue(id, out EnemyDefinition enemy) ? enemy : null;
        }

        /// <summary>
        /// Checks cross-references and common authoring mistakes. Returns human-readable errors (empty = valid).
        /// Run from the editor via the "Astro Idle/Validate Content" menu; also runs at game start.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (Ship == null) errors.Add($"No ShipDefinition found in Resources/{ShipPath}.");
            else ValidateTracks("Ship '" + Ship.Id + "'", Ship.Upgrades, errors);

            if (Waves == null) errors.Add($"No WaveConfig found in Resources/{ConfigPath}.");
            if (Economy == null) errors.Add($"No EconomyConfig found in Resources/{ConfigPath}.");
            else ValidateEconomy(errors);
            if (AutoBuy == null) errors.Add($"No AutoBuyConfig found in Resources/{ConfigPath}.");
            else ValidateAutoBuy(errors);

            CheckUniqueIds(Weapons, w => w.Id, "Weapon", errors);
            CheckUniqueIds(Enemies, e => e.Id, "Enemy", errors);
            CheckUniqueIds(Milestones, m => m.Id, "Milestone", errors);

            if (!Weapons.Any(w => w.CanBeStartingWeapon)) errors.Add("At least one weapon must have 'Can Be Starting Weapon' enabled.");

            foreach (WeaponDefinition weapon in Weapons)
            {
                ValidateTracks("Weapon '" + weapon.Id + "'", weapon.Upgrades, errors);
                if (weapon.GetBaseStat(StatId.FireRate) <= 0f) errors.Add($"Weapon '{weapon.Id}' must have a fire rate > 0.");
            }

            foreach (EnemyDefinition enemy in Enemies)
            {
                if (string.IsNullOrEmpty(enemy.SplitIntoEnemyId) || enemy.SplitCount <= 0) continue;
                if (enemy.SplitIntoEnemyId == enemy.Id) errors.Add($"Enemy '{enemy.Id}' cannot split into itself (infinite split).");
                else if (GetEnemy(enemy.SplitIntoEnemyId) == null) errors.Add($"Enemy '{enemy.Id}' splits into unknown enemy '{enemy.SplitIntoEnemyId}'.");
            }

            if (Waves != null)
            {
                if (Waves.SpawnTable.Count == 0) errors.Add("WaveConfig spawn table is empty.");
                foreach (SpawnEntry entry in Waves.SpawnTable)
                {
                    if (GetEnemy(entry.EnemyId) == null) errors.Add($"WaveConfig spawn table references unknown enemy '{entry.EnemyId}'.");
                }

                if (Waves.BossEveryNWaves > 0 && GetEnemy(Waves.BossEnemyId) == null)
                {
                    errors.Add($"WaveConfig boss enemy '{Waves.BossEnemyId}' does not exist.");
                }
            }

            foreach (MilestoneDefinition milestone in Milestones)
            {
                if (milestone.TargetValue <= 0d) errors.Add($"Milestone '{milestone.Id}' must have a target value > 0.");
                if (!string.IsNullOrEmpty(milestone.UnlockWeaponId) && GetWeapon(milestone.UnlockWeaponId) == null)
                {
                    errors.Add($"Milestone '{milestone.Id}' unlocks unknown weapon '{milestone.UnlockWeaponId}'.");
                }
            }

            return errors;
        }

        private void ValidateEconomy(List<string> errors)
        {
            if (Economy.WeaponUnlockCost <= 0d) errors.Add("EconomyConfig weapon unlock cost must be > 0.");
            if (Economy.WeaponUnlockCostGrowth < 1f) errors.Add("EconomyConfig weapon unlock cost growth must be >= 1.");

            CheckUniqueIds(Economy.UpgradeCostCurves, c => c.Id, "Cost curve", errors);
            foreach (CostCurve curve in Economy.UpgradeCostCurves)
            {
                if (curve.BaseCost <= 0d) errors.Add($"Cost curve '{curve.Id}' base cost must be > 0.");
                if (curve.CostGrowth < 1f) errors.Add($"Cost curve '{curve.Id}' cost growth must be >= 1.");
            }
        }

        private void ValidateAutoBuy(List<string> errors)
        {
            if (AutoBuy.TickIntervalSeconds <= 0f) errors.Add("AutoBuyConfig tick interval must be > 0.");
            if (AutoBuy.MaxPurchasesPerTick <= 0) errors.Add("AutoBuyConfig max purchases per tick must be > 0.");

            foreach (AutoBuyStrategy strategy in (AutoBuyStrategy[])Enum.GetValues(typeof(AutoBuyStrategy)))
            {
                AutoBuyProfile profile = AutoBuy.GetProfile(strategy);
                if (profile == null)
                {
                    errors.Add($"AutoBuyConfig has no profile for strategy '{strategy}'.");
                    continue;
                }

                bool anyPositive = false;
                foreach (AutoBuyCategory category in (AutoBuyCategory[])Enum.GetValues(typeof(AutoBuyCategory)))
                {
                    float weight = profile.GetWeight(category);
                    if (weight < 0f) errors.Add($"AutoBuy profile '{strategy}' has a negative {category} weight.");
                    anyPositive |= weight > 0f;
                }
                if (!anyPositive) errors.Add($"AutoBuy profile '{strategy}' has no positive weights, so it would never buy anything.");
            }
        }

        private void ValidateTracks(string owner, IReadOnlyList<UpgradeTrack> tracks, List<string> errors)
        {
            var seen = new HashSet<string>();
            foreach (UpgradeTrack track in tracks)
            {
                if (string.IsNullOrEmpty(track.Id)) errors.Add($"{owner} has an upgrade with an empty id.");
                else if (!seen.Add(track.Id)) errors.Add($"{owner} has duplicate upgrade id '{track.Id}'.");

                if (!string.IsNullOrEmpty(track.CostCurveId))
                {
                    // Shared curve: its own values are validated in ValidateEconomy.
                    if (Economy != null && Economy.GetCostCurve(track.CostCurveId) == null)
                    {
                        errors.Add($"{owner} upgrade '{track.Id}' uses unknown cost curve '{track.CostCurveId}'.");
                    }
                    continue;
                }

                if (track.CostGrowth < 1f) errors.Add($"{owner} upgrade '{track.Id}' cost growth must be >= 1.");
                if (track.BaseCost <= 0d) errors.Add($"{owner} upgrade '{track.Id}' base cost must be > 0.");
            }
        }

        private static void CheckUniqueIds<T>(IEnumerable<T> items, Func<T, string> getId, string label, List<string> errors)
        {
            var seen = new HashSet<string>();
            foreach (T item in items)
            {
                string id = getId(item);
                if (string.IsNullOrEmpty(id)) errors.Add($"{label} asset has an empty id.");
                else if (!seen.Add(id)) errors.Add($"Duplicate {label} id '{id}'.");
            }
        }

        private static Dictionary<string, T> BuildLookup<T>(IEnumerable<T> items, Func<T, string> getId)
        {
            var lookup = new Dictionary<string, T>();
            foreach (T item in items)
            {
                string id = getId(item);
                // Duplicates are reported by Validate(); keep the first so lookups stay deterministic.
                if (!string.IsNullOrEmpty(id) && !lookup.ContainsKey(id)) lookup.Add(id, item);
            }
            return lookup;
        }
    }
}
