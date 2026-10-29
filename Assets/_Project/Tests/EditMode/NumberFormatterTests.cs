using AstroIdle.Utilities;
using NUnit.Framework;

namespace AstroIdle.Tests
{
    public class NumberFormatterTests
    {
        [TestCase(0d, "0")]
        [TestCase(999d, "999")]
        [TestCase(999.9d, "999")]
        [TestCase(1000d, "1.00K")]
        [TestCase(1234d, "1.23K")]
        [TestCase(12345d, "12.3K")]
        [TestCase(999999d, "999K")]
        [TestCase(1000000d, "1.00M")]
        [TestCase(2500000000d, "2.50B")]
        [TestCase(-1500d, "-1.50K")]
        public void Format_UsesSuffixesAndTruncates(double value, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Format(value));
        }

        [Test]
        public void Format_BeyondSuffixes_UsesScientific()
        {
            Assert.AreEqual("1.50e36", NumberFormatter.Format(1.5e36));
        }

        [Test]
        public void FormatRate_ShowsOneDecimalForSmallValues()
        {
            Assert.AreEqual("2.5", NumberFormatter.FormatRate(2.55));
            Assert.AreEqual("150", NumberFormatter.FormatRate(150.7));
        }

        [Test]
        public void FormatDuration_PicksLargestUnits()
        {
            Assert.AreEqual("45s", NumberFormatter.FormatDuration(45));
            Assert.AreEqual("2m 05s", NumberFormatter.FormatDuration(125));
            Assert.AreEqual("3h 07m", NumberFormatter.FormatDuration(3 * 3600 + 7 * 60));
        }
    }
}
