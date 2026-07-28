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

    public async Task<string> ExportAsync(
        IReadOnlyCollection<string> selectedTechnologyIds,
        IEnumerable<CheckResultRow> results,
        CancellationToken cancellationToken)
    {
        string reportDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsDevInspector",
            "Reports");
        Directory.CreateDirectory(reportDirectory);

        string reportPath = Path.Combine(reportDirectory, $"scan-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json");
        ScanReport report = new()
        {
            CreatedAt = DateTimeOffset.UtcNow,
            SelectedTechnologyIds = selectedTechnologyIds.ToArray(),
            Results = results.Select(result => result.ToReportRow()).ToArray()
        };

        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions), cancellationToken);

        return reportPath;
    }
}

