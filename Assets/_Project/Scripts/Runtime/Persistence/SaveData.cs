using System;
using System.Collections.Generic;

namespace AstroIdle.Persistence
{
    /// <summary>
    /// Everything persisted between sessions, serialized to JSON with JsonUtility.
    /// This is a plain data object: services (wallet, upgrades, ...) read and write it, SaveService stores it.
    ///
    /// Compatibility rules: only ADD fields (with sensible defaults). If you must rename/remove/re-shape a
    /// field, bump <see cref="CurrentVersion"/> and migrate in SaveService.Migrate().
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        // ---- Economy ----
        public double credits;
        public double totalCreditsEarned;
        public double creditsPerSecond;

        // ---- Progress ----
        public long totalKills;
        public int currentWave = 1;
        public int highestWave = 1;
        public bool autoAdvance = true;

        // ---- Ownership ----
        public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();
        public List<string> unlockedWeapons = new List<string>();
        public List<string> ownedWeapons = new List<string>();
        public List<string> completedMilestones = new List<string>();

        // ---- Meta ----
        /// <summary>UTC DateTime ticks of the last save; drives offline earnings.</summary>
        public long lastSaveUtcTicks;

        public static string UpgradeKey(string ownerId, string trackId)
        {
            return ownerId + "." + trackId;
        }

        public int GetUpgradeLevel(string key)
        {
            for (int i = 0; i < upgradeLevels.Count; i++)
            {
                if (upgradeLevels[i].key == key) return upgradeLevels[i].level;
            }
            return 0;
        }

        public void SetUpgradeLevel(string key, int level)
        {
            for (int i = 0; i < upgradeLevels.Count; i++)
            {
                if (upgradeLevels[i].key == key)
                {
                    upgradeLevels[i].level = level;
                    return;
                }
            }
            upgradeLevels.Add(new UpgradeLevelEntry { key = key, level = level });
        }
    }
}
