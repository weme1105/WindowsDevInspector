using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App;

public sealed class PackageInstallationCandidateSelector(ApprovedInstallationCatalog catalog)
{
    public IReadOnlyList<PackageInstallationCandidate> GetCandidates(IEnumerable<CheckResultRow> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        CheckResultRow[] resultRows = results.ToArray();
        HashSet<string> availablePackageChecks = resultRows
            .Where(result => result.Severity.Equals(
                CheckSeverity.Pass.ToString(),
                StringComparison.OrdinalIgnoreCase))
            .Select(result => result.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<PackageInstallationCandidate> candidates = [];
        HashSet<(InstallationSource Source, string PackageId)> seenPackages =
            new(new PackageKeyComparer());

        foreach (CheckResultRow result in resultRows)
        {
            if (result.Severity.Equals(CheckSeverity.Pass.ToString(), StringComparison.OrdinalIgnoreCase)
                || !catalog.TryGetByDiagnosticCheckId(result.Id, out ApprovedInstallationPackage? package)
                || package is null
                || !availablePackageChecks.Contains(package.AvailabilityCheckId)
                || !seenPackages.Add((package.Source, package.PackageId)))
            {
                continue;
            }

            candidates.Add(new PackageInstallationCandidate
            {
                DiagnosticCheckId = result.Id,
                PackageId = package.PackageId,
                DisplayName = package.DisplayName,
                Source = package.Source,
                Action = InstallationAction.Install,
                Risk = package.Risk,
                CurrentValue = result.FullCurrentValue,
                Impact = result.Impact
            });
        }

        return candidates;
    }

    private sealed class PackageKeyComparer : IEqualityComparer<(InstallationSource Source, string PackageId)>
    {
        public bool Equals(
            (InstallationSource Source, string PackageId) x,
            (InstallationSource Source, string PackageId) y)
        {
            return x.Source == y.Source
                && StringComparer.OrdinalIgnoreCase.Equals(x.PackageId, y.PackageId);
        }

        public int GetHashCode((InstallationSource Source, string PackageId) key)
        {
            return HashCode.Combine(
                key.Source,
                StringComparer.OrdinalIgnoreCase.GetHashCode(key.PackageId));
        }
    }
}
