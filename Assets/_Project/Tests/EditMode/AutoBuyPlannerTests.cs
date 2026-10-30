using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Progression;
using NUnit.Framework;

namespace AstroIdle.Tests
{
    public class AutoBuyPlannerTests
    {
        private static readonly AutoBuyProfile EvenProfile =
            new AutoBuyProfile(AutoBuyStrategy.Balanced, 1f, 1f, 1f, 1f, 1f);

        [Test]
        public void PickBest_WithEqualWeights_PicksCheapest()
        {
            var candidates = new List<AutoBuyCandidate>
            {
                new AutoBuyCandidate(AutoBuyCategory.WeaponUpgrade, 30),
                new AutoBuyCandidate(AutoBuyCategory.ShipDefense, 20),
                new AutoBuyCandidate(AutoBuyCategory.WeaponUnlock, 60),
            };
            Assert.AreEqual(1, AutoBuyPlanner.PickBest(candidates, EvenProfile));
        }

        [Test]
        public void PickBest_HigherWeightMakesPricierItemWin()
        {
            // Unlock costs 60 but weight 4 -> score 15, beats a 20-credit upgrade at weight 1.
            var profile = new AutoBuyProfile(AutoBuyStrategy.Attack, 1f, 4f, 1f, 1f, 1f);
            var candidates = new List<AutoBuyCandidate>
            {
                new AutoBuyCandidate(AutoBuyCategory.WeaponUpgrade, 20),
                new AutoBuyCandidate(AutoBuyCategory.WeaponUnlock, 60),
            };
            Assert.AreEqual(1, AutoBuyPlanner.PickBest(candidates, profile));
        }

        [Test]
        public void PickBest_SkipsZeroWeightCategoriesAndHandlesEmpty()
        {
            var noShip = new AutoBuyProfile(AutoBuyStrategy.Attack, 1f, 1f, 0f, 0f, 0f);
            var candidates = new List<AutoBuyCandidate>
            {
                new AutoBuyCandidate(AutoBuyCategory.ShipDefense, 1),
                new AutoBuyCandidate(AutoBuyCategory.WeaponUpgrade, 500),
            };
            Assert.AreEqual(1, AutoBuyPlanner.PickBest(candidates, noShip));
            Assert.AreEqual(-1, AutoBuyPlanner.PickBest(new List<AutoBuyCandidate>(), EvenProfile));

            var onlyShip = new List<AutoBuyCandidate> { new AutoBuyCandidate(AutoBuyCategory.ShipEconomy, 5) };
            Assert.AreEqual(-1, AutoBuyPlanner.PickBest(onlyShip, noShip));
        }

        [TestCase(StatId.MaxHull, AutoBuyCategory.ShipDefense)]
        [TestCase(StatId.HullRegen, AutoBuyCategory.ShipDefense)]
        [TestCase(StatId.MoveSpeed, AutoBuyCategory.ShipDefense)]
        [TestCase(StatId.CreditMultiplier, AutoBuyCategory.ShipEconomy)]
        [TestCase(StatId.GlobalDamageMultiplier, AutoBuyCategory.ShipOffense)]
        [TestCase(StatId.GlobalFireRateMultiplier, AutoBuyCategory.ShipOffense)]
        public void CategoryFor_ShipTracks(StatId stat, AutoBuyCategory expected)
        {
            Assert.AreEqual(expected, AutoBuyPlanner.CategoryFor(stat, true));
        }

        [Test]
        public void CategoryFor_WeaponTracksAreAlwaysWeaponUpgrades()
        {
            Assert.AreEqual(AutoBuyCategory.WeaponUpgrade, AutoBuyPlanner.CategoryFor(StatId.Damage, false));
            Assert.AreEqual(AutoBuyCategory.WeaponUpgrade, AutoBuyPlanner.CategoryFor(StatId.SplashRadius, false));
        }
    }
}
