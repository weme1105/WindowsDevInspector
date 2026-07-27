using WindowsDevInspector.Core;

namespace WindowsDevInspector.Core.Tests;

public sealed class CheckCatalogTests
{
    [Fact]
    public void ResolveChecks_IncludesCommonChecks()
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();

        IReadOnlyList<CheckDefinition> checks = catalog.ResolveChecks([]);

        Assert.Contains(checks, check => check.Id == "common.windows-version");
        Assert.Contains(checks, check => check.Id == "common.git");
    }

    [Fact]
    public void ResolveChecks_DeduplicatesSharedChecks()
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();

        IReadOnlyList<CheckDefinition> checks = catalog.ResolveChecks(["angular", "vue", "nodejs"], includeCommonChecks: false);

        Assert.Equal(1, checks.Count(check => check.Id == "frontend.node-cli"));
        Assert.Equal(1, checks.Count(check => check.Id == "frontend.npm-cli"));
        Assert.Contains(checks, check => check.Id == "frontend.angular-cli");
        Assert.Contains(checks, check => check.Id == "frontend.vite-cli");
    }
}
