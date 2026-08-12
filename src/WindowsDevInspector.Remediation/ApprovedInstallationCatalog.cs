namespace WindowsDevInspector.Remediation;

public sealed class ApprovedInstallationCatalog
{
    private readonly IReadOnlyDictionary<(InstallationSource Source, string PackageId), ApprovedInstallationPackage> packages;
    private readonly IReadOnlyDictionary<string, ApprovedInstallationPackage> packagesByDiagnosticCheckId;

    public ApprovedInstallationCatalog(IEnumerable<ApprovedInstallationPackage> packages)
    {
        ArgumentNullException.ThrowIfNull(packages);

        this.packages = packages.ToDictionary(
            package => (package.Source, package.PackageId),
            new PackageKeyComparer());
        packagesByDiagnosticCheckId = this.packages.Values.ToDictionary(
            package => package.DiagnosticCheckId,
            StringComparer.OrdinalIgnoreCase);
    }

    public bool IsApproved(string packageId, InstallationSource source)
    {
        return packages.ContainsKey((source, packageId));
    }

    public ApprovedInstallationPackage GetRequired(string packageId, InstallationSource source)
    {
        if (packages.TryGetValue((source, packageId), out ApprovedInstallationPackage? package))
        {
            return package;
        }

        throw new InvalidOperationException(
            $"Package '{packageId}' from source '{source}' is not approved for installation.");
    }

    public bool TryGetByDiagnosticCheckId(
        string diagnosticCheckId,
        out ApprovedInstallationPackage? package)
    {
        return packagesByDiagnosticCheckId.TryGetValue(diagnosticCheckId, out package);
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
