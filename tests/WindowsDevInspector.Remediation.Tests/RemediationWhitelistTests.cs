using WindowsDevInspector.Core;

namespace WindowsDevInspector.Remediation.Tests;

public sealed class RemediationWhitelistTests
{
    [Fact]
    public void IsAllowed_ReturnsTrueForWhitelistedRemediation()
    {
        RemediationWhitelist whitelist = new([
            new RemediationDefinition
            {
                Id = "enable-long-paths",
                DisplayName = "Enable Windows long paths",
                Description = "Enables Windows long path support.",
                Risk = RiskLevel.Low
            }
        ]);

        Assert.True(whitelist.IsAllowed("ENABLE-LONG-PATHS"));
    }

    [Fact]
    public void GetRequired_RejectsUnknownRemediation()
    {
        RemediationWhitelist whitelist = new([]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => whitelist.GetRequired("run-arbitrary-command"));

        Assert.Contains("not whitelisted", exception.Message);
    }
}
