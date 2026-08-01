using System.Runtime.InteropServices;
using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class ProcessorArchitectureCheck : IEnvironmentCheck
{
    public string Id => "common.processor-architecture";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Architecture architecture = RuntimeInformation.OSArchitecture;

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "Common",
            Name = "Processor architecture",
            Severity = CheckSeverity.Info,
            CurrentValue = architecture.ToString(),
            ExpectedValue = "x64 or Arm64 Windows development workstation",
            Impact = "Processor architecture affects SDK, emulator, container, and native build tooling compatibility.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }
}
