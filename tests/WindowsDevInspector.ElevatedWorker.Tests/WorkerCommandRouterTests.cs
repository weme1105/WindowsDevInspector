using WindowsDevInspector.ElevatedWorker;

namespace WindowsDevInspector.ElevatedWorker.Tests;

public sealed class WorkerCommandRouterTests
{
    [Theory]
    [InlineData(WorkerCommandMode.Remediation, 11)]
    [InlineData(WorkerCommandMode.Rollback, 22)]
    [InlineData(WorkerCommandMode.Installation, 33)]
    public async Task RouteAsync_InvokesOnlyTypedHandler(WorkerCommandMode mode, int expectedExitCode)
    {
        List<string> calls = [];
        WorkerCommandRouter router = new(
            path => Complete("remediation", path, 11),
            path => Complete("rollback", path, 22),
            path => Complete("installation", path, 33));
        WorkerCommand command = new()
        {
            Mode = mode,
            InputPath = "input.json"
        };

        int exitCode = await router.RouteAsync(command);

        Assert.Equal(expectedExitCode, exitCode);
        Assert.Single(calls);

        Task<int> Complete(string handler, string path, int result)
        {
            calls.Add($"{handler}:{path}");
            return Task.FromResult(result);
        }
    }
}
