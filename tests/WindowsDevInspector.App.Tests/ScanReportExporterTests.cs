using System.Text.Json;
using WindowsDevInspector.App;
using WindowsDevInspector.Core;

namespace WindowsDevInspector.App.Tests;

public sealed class ScanReportExporterTests : IDisposable
{
    private readonly string reportDirectory = Path.Combine(
        Path.GetTempPath(),
        "WindowsDevInspector.App.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ExportAsync_WritesReportToConfiguredDirectoryWithStableTimestamp()
    {
        DateTimeOffset createdAt = new(2026, 7, 29, 10, 11, 12, TimeSpan.Zero);
        ScanReportExporter exporter = new(reportDirectory, () => createdAt);

        string reportPath = await exporter.ExportAsync(
            ["go", "dotnet"],
            [
                new CheckResultRow(new CheckResult
                {
                    Id = "backend.go-cli",
                    Category = "Backend",
                    Name = "Go CLI",
                    Severity = CheckSeverity.Warning,
                    CurrentValue = "go command not found",
                    ExpectedValue = "go version is available",
                    Impact = "Go projects cannot build.",
                    CanFix = false
                })
            ],
            CancellationToken.None);

        Assert.Equal(Path.Combine(reportDirectory, "scan-report-20260729101112.json"), reportPath);
        Assert.True(File.Exists(reportPath));
    }

    [Fact]
    public async Task ExportAsync_SerializesSelectedTechnologiesAndFullResultValues()
    {
        DateTimeOffset createdAt = new(2026, 7, 29, 10, 11, 12, TimeSpan.Zero);
        ScanReportExporter exporter = new(reportDirectory, () => createdAt);
        CheckResultRow row = new(new CheckResult
        {
            Id = "common.path-invalid-entries",
            Category = "Common",
            Name = "PATH invalid entries",
            Severity = CheckSeverity.Warning,
            CurrentValue = "First line\r\nSecond line",
            ExpectedValue = "All PATH entries exist",
            Impact = "Invalid entries can slow command lookup.",
            CanFix = true,
            Risk = RiskLevel.Low,
            RemediationId = "fix-path"
        });

        string reportPath = await exporter.ExportAsync(["dotnet"], [row], CancellationToken.None);

        string json = await File.ReadAllTextAsync(reportPath);
        ScanReport? report = JsonSerializer.Deserialize<ScanReport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(report);
        Assert.Equal(createdAt, report.CreatedAt);
        Assert.Equal(["dotnet"], report.SelectedTechnologyIds);
        CheckResultReportRow reportRow = Assert.Single(report.Results);
        Assert.Equal("common.path-invalid-entries", reportRow.Id);
        Assert.Equal("First line\r\nSecond line", reportRow.CurrentValue);
        Assert.True(reportRow.CanFix);
        Assert.Equal("fix-path", reportRow.RemediationId);
    }

    public void Dispose()
    {
        if (Directory.Exists(reportDirectory))
        {
            Directory.Delete(reportDirectory, recursive: true);
        }
    }
}

