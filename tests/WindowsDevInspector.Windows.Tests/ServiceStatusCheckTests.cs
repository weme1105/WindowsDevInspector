using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class ServiceStatusCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenExpectedStatusMatches()
    {
        ServiceStatusCheck check = new(
            "devops.hns",
            "DevOps",
            "Host Network Service",
            "hns",
            new FakeServiceReader(new ServiceReadResult { Exists = true, Status = "Running" }),
            expectedStatus: "Running",
            severityWhenUnexpected: CheckSeverity.Warning);

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Equal("Running", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsConfiguredSeverityWhenExpectedStatusDoesNotMatch()
    {
        ServiceStatusCheck check = new(
            "devops.docker-service",
            "DevOps",
            "Docker Desktop service",
            "com.docker.service",
            new FakeServiceReader(new ServiceReadResult { Exists = true, Status = "Stopped" }),
            expectedStatus: "Running",
            severityWhenUnexpected: CheckSeverity.Warning,
            missingImpact: "Docker Desktop service should be running.");

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Equal("Stopped", result.CurrentValue);
        Assert.Contains("should be running", result.Impact);
    }

    private sealed class FakeServiceReader(ServiceReadResult result) : IServiceReader
    {
        public ServiceReadResult ReadService(string serviceName)
        {
            return result;
        }
    }
}
