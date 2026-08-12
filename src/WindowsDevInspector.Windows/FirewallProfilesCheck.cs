using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class FirewallProfilesCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "security.firewall-profiles";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "netsh",
            "advfirewall show allprofiles state",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        if (!result.Succeeded)
        {
            return new CheckResult
            {
                Id = Id,
                Category = "Security",
                Name = "Firewall profiles",
                Severity = CheckSeverity.Warning,
                CurrentValue = FailureMessage(result),
                ExpectedValue = "netsh advfirewall can inspect Domain, Private, and Public profile states",
                Impact = "Windows Firewall profile state could not be inspected. Local development networking issues may be harder to diagnose.",
                CanFix = false,
                Risk = RiskLevel.None
            };
        }

        FirewallProfileState[] profiles = ParseProfiles(result.StandardOutput);
        bool hasProfiles = profiles.Length > 0;
        bool allProfilesOn = hasProfiles && profiles.All(profile => profile.Enabled);

        return new CheckResult
        {
            Id = Id,
            Category = "Security",
            Name = "Firewall profiles",
            Severity = allProfilesOn ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = hasProfiles ? string.Join("; ", profiles.Select(profile => $"{profile.Name}: {profile.State}")) : "No profile state lines found",
            ExpectedValue = "Domain, Private, and Public firewall profile states are inspectable",
            Impact = allProfilesOn
                ? "Windows Firewall profiles are enabled and inspectable."
                : "One or more Windows Firewall profiles appear disabled or could not be parsed. This can affect local service exposure and network troubleshooting.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static FirewallProfileState[] ParseProfiles(string output)
    {
        List<FirewallProfileState> profiles = [];
        string? currentProfile = null;

        foreach (string line in output.SplitLines())
        {
            if (line.EndsWith("Profile Settings:", StringComparison.OrdinalIgnoreCase))
            {
                currentProfile = line.Replace("Settings:", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                continue;
            }

            if (currentProfile is not null && line.StartsWith("State", StringComparison.OrdinalIgnoreCase))
            {
                string state = line["State".Length..].Trim();
                if (!string.IsNullOrWhiteSpace(state))
                {
                    profiles.Add(new FirewallProfileState(currentProfile, state));
                }

                currentProfile = null;
            }
        }

        return profiles.ToArray();
    }

    private static string FailureMessage(CommandRunResult result)
    {
        if (result.TimedOut)
        {
            return "Timed out";
        }

        if (result.ErrorMessage is not null)
        {
            return result.ErrorMessage;
        }

        return string.IsNullOrWhiteSpace(result.StandardError)
            ? $"Exit code {result.ExitCode}"
            : result.StandardError.Trim();
    }

    private sealed record FirewallProfileState(string Name, string State)
    {
        public bool Enabled => State.Equals("ON", StringComparison.OrdinalIgnoreCase);
    }
}
