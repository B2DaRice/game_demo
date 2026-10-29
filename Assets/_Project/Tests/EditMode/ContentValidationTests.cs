using AstroIdle.Content;
using NUnit.Framework;

namespace AstroIdle.Tests
{
    /// <summary>Guards the shipped content: fails if any asset in Resources/Content is broken.</summary>
    public class ContentValidationTests
    {
        [Test]
        public void ShippedContent_IsValid()
        {
            ContentDatabase content = ContentDatabase.LoadFromResources();
            var errors = content.Validate();
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void ShippedContent_HasCoreAssets()
        {
            ContentDatabase content = ContentDatabase.LoadFromResources();
            Assert.IsNotNull(content.Ship, "Ship definition missing");
            Assert.IsNotNull(content.Waves, "Wave config missing");
            Assert.IsNotNull(content.Economy, "Economy config missing");
            Assert.IsNotEmpty(content.Weapons, "No weapons");
            Assert.IsNotEmpty(content.Enemies, "No enemies");
        }
    }
}
