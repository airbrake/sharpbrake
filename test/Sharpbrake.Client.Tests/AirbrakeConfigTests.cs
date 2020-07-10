using System.Collections.Generic;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="AirbrakeConfig"/> class.
    /// </summary>
    public class AirbrakeConfigTests
    {
        [Fact]
        public void Ctor_ShouldInitializeListBasedProperties()
        {
            var config = new AirbrakeConfig();

            Assert.NotNull(config);
            Assert.NotNull(config.IgnoreEnvironments);
            Assert.NotNull(config.Allowlist);
            Assert.NotNull(config.Blocklist);
        }

        [Fact]
        public void Load_ShouldInitializeConfigFromDictionary()
        {
            var settings = new Dictionary<string, string>
            {
                {"Airbrake.ProjectId", "127348"},
                {"Airbrake:ProjectKey", "e2046ca6e4e9214b24ad252e3c99a0f6"},
                {"Airbrake:Environment", "test"},
                {"IgnoreEnvironments", "test,dev" },
                {"Blocklist", "password" },
                {"Environment", "dev"},
                {"NonExistingProperty", "value"}
            };

            var config = AirbrakeConfig.Load(settings);

            Assert.NotNull(config);
            Assert.True(config.ProjectId == "127348");
            Assert.True(config.ProjectKey == "e2046ca6e4e9214b24ad252e3c99a0f6");
            Assert.True(config.Environment == "dev");

            Assert.NotNull(config.IgnoreEnvironments);
            Assert.True(config.IgnoreEnvironments.Count == 2);
            Assert.True(config.IgnoreEnvironments[0] == "test");
            Assert.True(config.IgnoreEnvironments[1] == "dev");

            Assert.NotNull(config.Blocklist);
            Assert.True(config.Blocklist.Count == 1);
            Assert.True(config.Blocklist[0] == "password");
        }
    }
}
