using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class BuiltInEnvironmentCheckFactoryTests
{
    [Theory]
    [InlineData("common.windows-version")]
    [InlineData("common.processor-architecture")]
    [InlineData("backend.dotnet-cli")]
    [InlineData("common.path-invalid-entries")]
    [InlineData("common.path-duplicate-entries")]
    [InlineData("common.chocolatey")]
    [InlineData("security.firewall-profiles")]
    [InlineData("security.code-integrity-events")]
    [InlineData("security.smart-app-control")]
    [InlineData("backend.dotnet-sdk")]
    [InlineData("backend.dotnet-runtime")]
    [InlineData("backend.aspnet-runtime")]
    [InlineData("backend.nuget-sources")]
    [InlineData("backend.visualstudio-buildtools")]
    [InlineData("frontend.node-cli")]
    [InlineData("frontend.npm-cli")]
    [InlineData("frontend.npm-global-prefix")]
    [InlineData("frontend.nvm")]
    [InlineData("frontend.electron-package")]
    [InlineData("frontend.angular-cli")]
    [InlineData("frontend.pnpm-cli")]
    [InlineData("frontend.yarn-cli")]
    [InlineData("backend.docker-cli")]
    [InlineData("backend.go-cli")]
    [InlineData("backend.python-cli")]
    [InlineData("backend.flask-package")]
    [InlineData("backend.php-cli")]
    [InlineData("backend.ruby-cli")]
    [InlineData("backend.rails-gem")]
    [InlineData("backend.rust-cli")]
    [InlineData("backend.cargo-cli")]
    [InlineData("database.sqlcmd-cli")]
    [InlineData("database.oracle-client")]
    [InlineData("devops.docker-cli")]
    [InlineData("devops.kubectl")]
    [InlineData("devops.github-cli")]
    [InlineData("devops.azure-cli")]
    [InlineData("devops.google-cloud-cli")]
    [InlineData("devops.terraform")]
    [InlineData("devops.jenkins")]
    [InlineData("devops.wsl")]
    [InlineData("devops.wsl-version")]
    [InlineData("devops.wsl-distros")]
    [InlineData("devops.virtual-machine-platform")]
    [InlineData("devops.hyper-v")]
    [InlineData("devops.winnat")]
    [InlineData("devops.hns")]
    [InlineData("devops.docker-service")]
    [InlineData("devops.localhost-bind")]
    [InlineData("install.pnpm-winget")]
    [InlineData("install.azure-cli-winget")]
    [InlineData("install.kubectl-winget")]
    [InlineData("install.terraform-winget")]
    [InlineData("database.ssms")]
    [InlineData("qa.postman")]
    [InlineData("qa.newman")]
    [InlineData("qa.browser-availability")]
    [InlineData("qa.pytest-package")]
    [InlineData("mobile.android-sdk")]
    [InlineData("mobile.adb")]
    [InlineData("mobile.dotnet-maui")]
    [InlineData("mobile.swift-cli")]
    [InlineData("desktop.visualstudio")]
    [InlineData("desktop.dotnet-desktop-runtime")]
    [InlineData("desktop.vscode")]
    [InlineData("desktop.windows-terminal")]
    [InlineData("desktop.powershell")]
    [InlineData("desktop.msbuild")]
    public void CreateAll_IncludesExpectedCliChecks(string checkId)
    {
        IReadOnlyDictionary<string, IEnvironmentCheck> checks = BuiltInEnvironmentCheckFactory.CreateAll();

        Assert.Contains(checkId, checks.Keys);
    }

    [Fact]
    public void CreateAll_IncludesExecutableCheckForEveryCatalogCheck()
    {
        CheckCatalog catalog = BuiltInCheckCatalog.Create();
        IReadOnlyDictionary<string, IEnvironmentCheck> executableChecks = BuiltInEnvironmentCheckFactory.CreateAll();

        string[] missingIds = catalog.Checks
            .Select(check => check.Id)
            .Where(checkId => !executableChecks.ContainsKey(checkId))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Empty(missingIds);
    }
}
