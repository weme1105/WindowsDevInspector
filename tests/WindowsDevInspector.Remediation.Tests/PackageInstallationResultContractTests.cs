using System.Text.Json;

namespace WindowsDevInspector.Remediation.Tests;

public sealed class PackageInstallationResultContractTests
{
    [Fact]
    public void Deserialize_OlderResultWithoutVerification_RemainsCompatible()
    {
        string json = """
            {
              "packageId": "pnpm.pnpm",
              "outcome": 1,
              "message": "Installed.",
              "commandPreview": {
                "packageId": "pnpm.pnpm",
                "source": 1,
                "fileName": "winget",
                "arguments": ["install"]
              }
            }
            """;

        PackageInstallationItemResult result = Assert.IsType<PackageInstallationItemResult>(
            JsonSerializer.Deserialize<PackageInstallationItemResult>(
                json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        Assert.Equal(PackageInstallationOutcome.Succeeded, result.Outcome);
        Assert.Null(result.Verification);
    }
}
