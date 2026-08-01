using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WindowsDevInspector.App;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App.Tests;

public sealed class RemediationCoordinatorTests
{
    [Fact]
    public async Task ExecuteElevatedRollbackAsync_PassesRollbackArgumentsAndReadsResultFile()
    {
        string workDirectory = CreateTempDirectory();
        string workerPath = Path.Combine(workDirectory, "WindowsDevInspector.ElevatedWorker.exe");
        string backupPath = Path.Combine(workDirectory, "Backups", "sample.backup.json");
        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
        File.WriteAllText(workerPath, "");
        File.WriteAllText(backupPath, "{}");

        FakeWorkerProcessRunner runner = new(startInfo =>
        {
            string[] arguments = SplitArguments(startInfo.Arguments);
            string resultPath = arguments[2];
            WorkerExecutionResult result = new()
            {
                PlanId = "rollback",
                Succeeded = true,
                Errors = [],
                Results =
                [
                    RemediationExecutionResult.Success(
                        "rollback-registry-dword",
                        "Rollback completed.")
                ]
            };

            File.WriteAllText(resultPath, JsonSerializer.Serialize(result));
            return 0;
        });
        RemediationCoordinator coordinator = new(runner, workerPath, workDirectory);

        WorkerExecutionResult rollbackResult = await coordinator.ExecuteElevatedRollbackAsync(backupPath);

        Assert.True(rollbackResult.Succeeded);
        ProcessStartInfo startInfo = Assert.Single(runner.StartInfos);
        Assert.Equal(workerPath, startInfo.FileName);
        Assert.Equal("runas", startInfo.Verb);
        Assert.True(startInfo.UseShellExecute);

        string[] passedArguments = SplitArguments(startInfo.Arguments);
        Assert.Equal("--rollback", passedArguments[0]);
        Assert.Equal(backupPath, passedArguments[1]);
        Assert.EndsWith(".result.json", passedArguments[2], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteElevatedRollbackAsync_ReturnsFailureWhenWorkerDoesNotWriteResultFile()
    {
        string workDirectory = CreateTempDirectory();
        string workerPath = Path.Combine(workDirectory, "WindowsDevInspector.ElevatedWorker.exe");
        string backupPath = Path.Combine(workDirectory, "sample.backup.json");
        File.WriteAllText(workerPath, "");
        File.WriteAllText(backupPath, "{}");

        RemediationCoordinator coordinator = new(
            new FakeWorkerProcessRunner(_ => 6),
            workerPath,
            workDirectory);

        WorkerExecutionResult rollbackResult = await coordinator.ExecuteElevatedRollbackAsync(backupPath);

        Assert.False(rollbackResult.Succeeded);
        string error = Assert.Single(rollbackResult.Errors);
        Assert.Contains("did not write rollback result file", error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Exit code: 6", error, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"WindowsDevInspector.App.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static string[] SplitArguments(string arguments)
    {
        List<string> values = [];
        bool inQuotes = false;
        StringBuilder current = new();

        foreach (char character in arguments)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddCurrent();
                continue;
            }

            current.Append(character);
        }

        AddCurrent();
        return values.ToArray();

        void AddCurrent()
        {
            if (current.Length == 0)
            {
                return;
            }

            values.Add(current.ToString());
            current.Clear();
        }
    }

    private sealed class FakeWorkerProcessRunner(Func<ProcessStartInfo, int> run) : IWorkerProcessRunner
    {
        public List<ProcessStartInfo> StartInfos { get; } = [];

        public Task<int> RunAsync(ProcessStartInfo startInfo)
        {
            StartInfos.Add(startInfo);
            return Task.FromResult(run(startInfo));
        }
    }
}
