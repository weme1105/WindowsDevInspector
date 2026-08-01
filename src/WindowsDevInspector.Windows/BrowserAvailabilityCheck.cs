using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class BrowserAvailabilityCheck(IFileSystem fileSystem) : IEnvironmentCheck
{
    private static readonly BrowserCandidate[] Candidates = [
        new("Edge", [
            "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
            "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe"
        ]),
        new("Chrome", [
            "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
            "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"
        ]),
        new("Firefox", [
            "C:\\Program Files\\Mozilla Firefox\\firefox.exe",
            "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe"
        ])
    ];

    public string Id => "qa.browser-availability";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string[] found = Candidates
            .Where(candidate => candidate.Paths.Any(fileSystem.FileExists))
            .Select(candidate => candidate.Name)
            .ToArray();

        bool anyFound = found.Length > 0;

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "QA",
            Name = "Browser availability",
            Severity = anyFound ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = anyFound ? string.Join(", ", found) : "No common browser executable found in standard install paths",
            ExpectedValue = "Edge, Chrome, or Firefox executable available",
            Impact = anyFound
                ? "At least one common browser is available for browser-based testing."
                : "Browser automation tools may need a browser installed or configured explicitly.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }

    private sealed record BrowserCandidate(string Name, IReadOnlyList<string> Paths);
}
