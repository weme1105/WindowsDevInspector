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
        Assert.Contains(checks, check => check.Id == "desktop.dotnet-desktop-runtime"
            && check.Category == "Common");
        Assert.Contains(checks, check => check.Id == "security.firewall-profiles");
        Assert.Contains(checks, check => check.Id == "security.code-integrity-events");
        Assert.Contains(checks, check => check.Id == "security.smart-app-control");
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

    [Theory]
    [InlineData("react")]
    [InlineData("nextjs")]
    [InlineData("tailwindcss")]
    public void ResolveChecks_IncludesFrontendBacklogTechnologyMappings(string technologyId)
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();

        IReadOnlyList<CheckDefinition> checks = catalog.ResolveChecks([technologyId], includeCommonChecks: false);

        Assert.Contains(checks, check => check.Id == "frontend.node-cli");
        Assert.Contains(checks, check => check.Id == "frontend.npm-cli");
    }

    [Theory]
    [InlineData("github-cli", "devops.github-cli")]
    [InlineData("chocolatey", "common.chocolatey")]
    [InlineData("google-cloud-cli", "devops.google-cloud-cli")]
    [InlineData("windows-terminal", "desktop.windows-terminal")]
    [InlineData("ssms", "database.ssms")]
    [InlineData("dotnet-desktop-runtime", "desktop.dotnet-desktop-runtime")]
    [InlineData("powershell", "desktop.powershell")]
    [InlineData("flask", "backend.python-cli")]
    [InlineData("pytest", "backend.python-cli")]
    [InlineData("php", "backend.php-cli")]
    [InlineData("rails", "backend.ruby-cli")]
    [InlineData("rust", "backend.rust-cli")]
    [InlineData("electron", "frontend.electron-package")]
    [InlineData("react-native", "mobile.android-sdk")]
    [InlineData("swift", "mobile.swift-cli")]
    public void ResolveChecks_IncludesToolBacklogTechnologyMappings(string technologyId, string expectedCheckId)
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();

        IReadOnlyList<CheckDefinition> checks = catalog.ResolveChecks([technologyId], includeCommonChecks: false);

        Assert.Contains(checks, check => check.Id == expectedCheckId);
    }

    [Fact]
    public void ResolveChecks_IncludesDeeperFrameworkPackageDiagnostics()
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();

        IReadOnlyList<CheckDefinition> checks = catalog.ResolveChecks(
            ["flask", "pytest", "rails", "rust"],
            includeCommonChecks: false);

        Assert.Contains(checks, check => check.Id == "backend.flask-package");
        Assert.Contains(checks, check => check.Id == "qa.pytest-package");
        Assert.Contains(checks, check => check.Id == "backend.rails-gem");
        Assert.Contains(checks, check => check.Id == "backend.cargo-cli");
    }

    [Fact]
    public void ResolveChecks_PowerShell7DoesNotIncludePackagePlanning()
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();

        IReadOnlyList<CheckDefinition> checks = catalog.ResolveChecks(["powershell7"], includeCommonChecks: false);

        CheckDefinition check = Assert.Single(checks);
        Assert.Equal("common.powershell7", check.Id);
    }
}
