namespace WindowsDevInspector.Core;

public sealed record EnvironmentScore
{
    public required int Score { get; init; }

    public required int TotalCount { get; init; }

    public required int CriticalCount { get; init; }

    public required int WarningCount { get; init; }

    public required int InfoCount { get; init; }

    public required int PassCount { get; init; }
}
