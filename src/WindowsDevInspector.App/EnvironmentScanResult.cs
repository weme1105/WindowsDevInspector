using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public sealed record EnvironmentScanResult
{
    public required IReadOnlyList<CheckResult> Results { get; init; }

    public required EnvironmentScore Score { get; init; }
}

