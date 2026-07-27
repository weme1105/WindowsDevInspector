namespace WindowsDevInspector.Windows.Tests;

public sealed class BuiltInEnvironmentCheckFactoryTests
{
    [Theory]
    [InlineData("backend.dotnet-cli")]
    [InlineData("common.path-invalid-entries")]
    [InlineData("common.path-duplicate-entries")]
    [InlineData("backend.dotnet-sdk")]
    [InlineData("backend.dotnet-runtime")]
    [InlineData("frontend.npm-cli")]
    [InlineData("frontend.npm-global-prefix")]
    [InlineData("frontend.nvm")]
    [InlineData("frontend.angular-cli")]
    [InlineData("frontend.pnpm-cli")]
    [InlineData("frontend.yarn-cli")]
    [InlineData("backend.docker-cli")]
    [InlineData("backend.go-cli")]
    [InlineData("backend.python-cli")]
    [InlineData("database.sqlcmd-cli")]
    [InlineData("database.oracle-client")]
    [InlineData("devops.docker-cli")]
    [InlineData("devops.kubectl")]
    [InlineData("devops.azure-cli")]
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
    [InlineData("database.ssms")]
    [InlineData("qa.postman")]
    [InlineData("qa.newman")]
    [InlineData("mobile.adb")]
    [InlineData("desktop.visualstudio")]
    [InlineData("desktop.vscode")]
    [InlineData("desktop.msbuild")]
    public void CreateAll_IncludesExpectedCliChecks(string checkId)
    {
        IReadOnlyDictionary<string, IEnvironmentCheck> checks = BuiltInEnvironmentCheckFactory.CreateAll();

        Assert.Contains(checkId, checks.Keys);
    }
}
