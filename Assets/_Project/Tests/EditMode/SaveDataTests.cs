using AstroIdle.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace AstroIdle.Tests
{
    public class SaveDataTests
    {
        [Test]
        public void UpgradeLevels_DefaultToZeroAndUpdateInPlace()
        {
            var save = new SaveData();
            string key = SaveData.UpgradeKey("ship", "hull");

            Assert.AreEqual(0, save.GetUpgradeLevel(key));
            save.SetUpgradeLevel(key, 3);
            save.SetUpgradeLevel(key, 4);
            Assert.AreEqual(4, save.GetUpgradeLevel(key));
            Assert.AreEqual(1, save.upgradeLevels.Count);
        }

        [Test]
        public void JsonRoundTrip_PreservesProgress()
        {
            var save = new SaveData
            {
                credits = 12345.5,
                totalKills = 42,
                currentWave = 7,
                highestWave = 9,
                autoAdvance = false,
            };
            save.SetUpgradeLevel("pulse_blaster.damage", 5);
            save.ownedWeapons.Add("pulse_blaster");
            save.completedMilestones.Add("first_contact");

            var loaded = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save));

            Assert.AreEqual(save.credits, loaded.credits);
            Assert.AreEqual(save.totalKills, loaded.totalKills);
            Assert.AreEqual(7, loaded.currentWave);
            Assert.AreEqual(9, loaded.highestWave);
            Assert.IsFalse(loaded.autoAdvance);
            Assert.AreEqual(5, loaded.GetUpgradeLevel("pulse_blaster.damage"));
            CollectionAssert.Contains(loaded.ownedWeapons, "pulse_blaster");
            CollectionAssert.Contains(loaded.completedMilestones, "first_contact");
        }
    }
}
