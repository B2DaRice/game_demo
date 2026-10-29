using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Gameplay.Waves;
using NUnit.Framework;
using UnityEngine;

namespace AstroIdle.Tests
{
    public class WaveMathTests
    {
        private WaveConfig config;

        [SetUp]
        public void SetUp()
        {
            // A fresh instance uses the field defaults declared in WaveConfig.cs.
            config = ScriptableObject.CreateInstance<WaveConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void EnemyCount_StartsAtBaseAndIsCapped()
        {
            Assert.AreEqual(config.BaseEnemyCount, WaveMath.EnemyCount(config, 1));
            Assert.AreEqual(config.BaseEnemyCount + config.EnemiesPerWave, WaveMath.EnemyCount(config, 2));
            Assert.AreEqual(config.MaxEnemiesPerWave, WaveMath.EnemyCount(config, 10000));
        }

        [Test]
        public void SpawnInterval_NeverGoesBelowMinimum()
        {
            Assert.AreEqual(config.BaseSpawnInterval, WaveMath.SpawnInterval(config, 1), 1e-4f);
            Assert.AreEqual(config.MinSpawnInterval, WaveMath.SpawnInterval(config, 10000), 1e-4f);
        }

        [Test]
        public void Scaling_IsIdentityOnWaveOneAndGrowsAfter()
        {
            var first = WaveMath.Scaling(config, 1);
            Assert.AreEqual(1f, first.HealthMultiplier, 1e-4f);
            Assert.AreEqual(1d, first.RewardMultiplier, 1e-6d);

            var later = WaveMath.Scaling(config, 11);
            Assert.Greater(later.HealthMultiplier, 1f);
            Assert.LessOrEqual(later.SpeedMultiplier, config.MaxSpeedMultiplier);
        }

        [Test]
        public void IsBossWave_EveryNthWave()
        {
            Assert.IsFalse(WaveMath.IsBossWave(config, config.BossEveryNWaves - 1));
            Assert.IsTrue(WaveMath.IsBossWave(config, config.BossEveryNWaves));
        }

        [Test]
        public void SpawnPicker_RespectsWaveWindowsAndWeights()
        {
            var table = new List<SpawnEntry>
            {
                new SpawnEntry("rock", 1, 0, 1f),
                new SpawnEntry("drone", 3, 0, 3f),
            };

            Assert.AreEqual("rock", SpawnPicker.Pick(table, 1, 0.99f), "drone is not eligible before wave 3");
            Assert.AreEqual("rock", SpawnPicker.Pick(table, 5, 0.1f), "roll in first quarter picks rock");
            Assert.AreEqual("drone", SpawnPicker.Pick(table, 5, 0.5f), "roll past rock's weight picks drone");
            Assert.IsNull(SpawnPicker.Pick(new List<SpawnEntry>(), 1, 0.5f));
        }
    }
}
