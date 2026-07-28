namespace WindowsDevInspector.App;

public sealed record ScanReport
{
    public required DateTimeOffset CreatedAt { get; init; }

    public required IReadOnlyList<string> SelectedTechnologyIds { get; init; }

    public required IReadOnlyList<CheckResultReportRow> Results { get; init; }
}
