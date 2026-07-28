using WindowsDevInspector.App;
using WindowsDevInspector.Core;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.App.Tests;

public sealed class EnvironmentScanServiceTests
{
    [Fact]
    public async Task RunScanAsync_ExecutesResolvedChecksAndSortsResults()
    {
        CheckCatalog catalog = new(
            [
                Technology("go", ["backend.go-cli", "backend.go-env"])
            ],
            [
                Check("backend.go-cli", "Go CLI", "Backend", CheckSeverity.Warning),
                Check("backend.go-env", "Go environment", "Backend"),
                Check("common.git", "Git CLI", "Common", CheckSeverity.Warning)
            ]);
        EnvironmentScanService service = new(catalog, new Dictionary<string, IEnvironmentCheck>(StringComparer.OrdinalIgnoreCase)
        {
            ["backend.go-cli"] = new FakeEnvironmentCheck(Result("backend.go-cli", CheckSeverity.Pass)),
            ["backend.go-env"] = new FakeEnvironmentCheck(Result("backend.go-env", CheckSeverity.Warning)),
            ["common.git"] = new FakeEnvironmentCheck(Result("common.git", CheckSeverity.Pass))
        });

        EnvironmentScanResult result = await service.RunScanAsync(["go"], CancellationToken.None);

        Assert.Collection(
            result.Results,
            check => Assert.Equal("backend.go-env", check.Id),
            check => Assert.Equal("backend.go-cli", check.Id),
            check => Assert.Equal("common.git", check.Id));
        Assert.Equal(85, result.Score.Score);
        Assert.Equal(1, result.Score.WarningCount);
        Assert.Equal(2, result.Score.PassCount);
    }

    [Fact]
    public async Task RunScanAsync_CreatesInfoResultForPendingCheck()
    {
        CheckCatalog catalog = new(
            [
                Technology("aspnetcore", ["backend.aspnet-runtime"])
            ],
            [
                Check("backend.aspnet-runtime", "ASP.NET Core runtime", "Backend")
            ]);
        EnvironmentScanService service = new(catalog, new Dictionary<string, IEnvironmentCheck>(StringComparer.OrdinalIgnoreCase));

        EnvironmentScanResult result = await service.RunScanAsync(["aspnetcore"], CancellationToken.None);

        CheckResult pendingResult = Assert.Single(result.Results);
        Assert.Equal("backend.aspnet-runtime", pendingResult.Id);
        Assert.Equal(CheckSeverity.Info, pendingResult.Severity);
        Assert.False(pendingResult.CanFix);
        Assert.Equal(97, result.Score.Score);
    }

    [Fact]
    public async Task RunScanAsync_ForwardsCancellationTokenToExecutableCheck()
    {
        CheckCatalog catalog = new(
            [],
            [
                Check("common.git", "Git CLI", "Common", CheckSeverity.Warning)
            ]);
        CapturingEnvironmentCheck executableCheck = new(Result("common.git", CheckSeverity.Pass));
        EnvironmentScanService service = new(catalog, new Dictionary<string, IEnvironmentCheck>(StringComparer.OrdinalIgnoreCase)
        {
            ["common.git"] = executableCheck
        });
        using CancellationTokenSource cancellationTokenSource = new();

        await service.RunScanAsync([], cancellationTokenSource.Token);

        Assert.Equal(cancellationTokenSource.Token, executableCheck.CapturedToken);
    }

    private static TechnologyDefinition Technology(string id, IReadOnlyCollection<string> checkIds)
    {
        return new TechnologyDefinition
        {
            Id = id,
            Name = id,
            GroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            CheckIds = new HashSet<string>(checkIds, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static CheckDefinition Check(
        string id,
        string name,
        string category,
        CheckSeverity severityWhenMissing = CheckSeverity.Info)
    {
        return new CheckDefinition
        {
            Id = id,
            Name = name,
            Category = category,
            SeverityWhenMissing = severityWhenMissing
        };
    }

    private static CheckResult Result(string id, CheckSeverity severity)
    {
        return new CheckResult
        {
            Id = id,
            Category = id.StartsWith("common.", StringComparison.OrdinalIgnoreCase) ? "Common" : "Backend",
            Name = id,
            Severity = severity,
            CurrentValue = "Current",
            ExpectedValue = "Expected",
            Impact = "Impact"
        };
    }

    private sealed class FakeEnvironmentCheck(CheckResult result) : IEnvironmentCheck
    {
        public string Id => result.Id;

        public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }
    }

    private sealed class CapturingEnvironmentCheck(CheckResult result) : IEnvironmentCheck
    {
        public string Id => result.Id;

        public CancellationToken CapturedToken { get; private set; }

        public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
        {
            CapturedToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
