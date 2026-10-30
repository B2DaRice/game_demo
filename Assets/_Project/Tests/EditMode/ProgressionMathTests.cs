using AstroIdle.Progression;
using NUnit.Framework;

namespace AstroIdle.Tests
{
    public class ProgressionMathTests
    {
        [Test]
        public void UpgradeCost_GrowsGeometricallyAndRoundsUp()
        {
            Assert.AreEqual(10d, ProgressionMath.UpgradeCost(10d, 1.2f, 0));
            Assert.AreEqual(12d, ProgressionMath.UpgradeCost(10d, 1.2f, 1));
            Assert.AreEqual(15d, ProgressionMath.UpgradeCost(10d, 1.2f, 2)); // 14.4 -> 15
        }

        [Test]
        public void WeaponUnlockCost_IsFlatWithGrowthOneAndScalesWithOwnedWeapons()
        {
            Assert.AreEqual(15d, ProgressionMath.WeaponUnlockCost(15d, 1f, 1));
            Assert.AreEqual(15d, ProgressionMath.WeaponUnlockCost(15d, 1f, 3));
            Assert.AreEqual(15d, ProgressionMath.WeaponUnlockCost(15d, 2f, 1), "first extra weapon uses the base price");
            Assert.AreEqual(30d, ProgressionMath.WeaponUnlockCost(15d, 2f, 2));
            Assert.AreEqual(60d, ProgressionMath.WeaponUnlockCost(15d, 2f, 3));
        }

        [Test]
        public void PickIndex_MapsRollAcrossRangeAndHandlesEdges()
        {
            Assert.AreEqual(0, ProgressionMath.PickIndex(3, 0f));
            Assert.AreEqual(1, ProgressionMath.PickIndex(3, 0.5f));
            Assert.AreEqual(2, ProgressionMath.PickIndex(3, 0.99f));
            Assert.AreEqual(2, ProgressionMath.PickIndex(3, 1f), "a roll of exactly 1 stays in range");
            Assert.AreEqual(-1, ProgressionMath.PickIndex(0, 0.5f));
        }

        [Test]
        public void EvaluateStat_AppliesFlatThenPercent()
        {
            Assert.AreEqual(10f, ProgressionMath.EvaluateStat(10f, 0f, 0f), 1e-4f);
            Assert.AreEqual(15f, ProgressionMath.EvaluateStat(10f, 5f, 0f), 1e-4f);
            Assert.AreEqual(30f, ProgressionMath.EvaluateStat(10f, 5f, 1f), 1e-4f);
        }

        [Test]
        public void OfflineEarnings_IsCappedAndScaledByEfficiency()
        {
            Assert.AreEqual(250d, OfflineEarnings.Calculate(100d, 10d, 0.25f, 8f));
            // 10 hours away with an 8 hour cap.
            Assert.AreEqual(8d * 3600d * 10d * 0.25d, OfflineEarnings.Calculate(10d * 3600d, 10d, 0.25f, 8f));
            Assert.AreEqual(0d, OfflineEarnings.Calculate(-5d, 10d, 0.25f, 8f));
            Assert.AreEqual(0d, OfflineEarnings.Calculate(100d, 0d, 0.25f, 8f));
        }
    }
}
