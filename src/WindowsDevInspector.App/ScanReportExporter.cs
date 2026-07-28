using System.IO;
using System.Text.Json;

namespace WindowsDevInspector.App;

public sealed class ScanReportExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly Func<DateTimeOffset> utcNowProvider;
    private readonly string reportDirectory;

    public ScanReportExporter()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WindowsDevInspector",
                "Reports"),
            () => DateTimeOffset.UtcNow)
    {
    }

    public ScanReportExporter(string reportDirectory, Func<DateTimeOffset> utcNowProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportDirectory);
        ArgumentNullException.ThrowIfNull(utcNowProvider);

        this.reportDirectory = reportDirectory;
        this.utcNowProvider = utcNowProvider;
    }

    public async Task<string> ExportAsync(
        IReadOnlyCollection<string> selectedTechnologyIds,
        IEnumerable<CheckResultRow> results,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(reportDirectory);

        DateTimeOffset createdAt = utcNowProvider();
        string reportPath = Path.Combine(reportDirectory, $"scan-report-{createdAt:yyyyMMddHHmmss}.json");
        ScanReport report = new()
        {
            CreatedAt = createdAt,
            SelectedTechnologyIds = selectedTechnologyIds.ToArray(),
            Results = results.Select(result => result.ToReportRow()).ToArray()
        };

        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions), cancellationToken);

        return reportPath;
    }
}
