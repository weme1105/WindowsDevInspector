using System.Text.Json;

namespace WindowsDevInspector.Remediation.Tests;

public sealed class InstallationPlanValidatorTests
{
    [Fact]
    public void Validate_RejectsMissingPlan()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());

        InstallationPlanValidationResult result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("missing", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsMoreThanOneItem()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = CreatePlan(
            Enumerable.Range(0, 2)
                .Select(_ => new InstallationPlanItem
                {
                    PackageId = "pnpm.pnpm",
                    Source = InstallationSource.Winget,
                    Action = InstallationAction.Install
                })
                .ToArray());

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("exactly one", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_AcceptsApprovedPackageFromApprovedSource()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = CreatePlan(
            new InstallationPlanItem
            {
                PackageId = "pnpm.pnpm",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            });

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsUnknownPackage()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = CreatePlan(
            new InstallationPlanItem
            {
                PackageId = "Unknown.ArbitraryPackage",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            });

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("not approved", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsUnknownSource()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = CreatePlan(
            new InstallationPlanItem
            {
                PackageId = "pnpm.pnpm",
                Source = (InstallationSource)999,
                Action = InstallationAction.Install
            });

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("source", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsUnknownAction()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = CreatePlan(
            new InstallationPlanItem
            {
                PackageId = "pnpm.pnpm",
                Source = InstallationSource.Winget,
                Action = (InstallationAction)999
            });

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("action", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsDuplicatePackageIgnoringCase()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = CreatePlan(
            new InstallationPlanItem
            {
                PackageId = "pnpm.pnpm",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            },
            new InstallationPlanItem
            {
                PackageId = "PNPM.PNPM",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            });

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsInvalidPlanId()
    {
        InstallationPlanValidator validator = new(BuiltInInstallationCatalog.Create());
        InstallationPlan plan = new()
        {
            PlanId = "user-controlled-plan-id",
            Items =
            [
                new InstallationPlanItem
                {
                    PackageId = "pnpm.pnpm",
                    Source = InstallationSource.Winget,
                    Action = InstallationAction.Install
                }
            ]
        };

        InstallationPlanValidationResult result = validator.Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("GUID", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Deserialize_RejectsUnmappedExecutableOrArguments()
    {
        string json = $$"""
            {
              "planId": "{{Guid.NewGuid():D}}",
              "items": [
                {
                  "packageId": "pnpm.pnpm",
                  "source": 1,
                  "action": 1,
                  "executable": "powershell.exe",
                  "arguments": ["-Command", "arbitrary"]
                }
              ]
            }
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<InstallationPlan>(json, JsonOptions));
    }

    [Fact]
    public void BuiltInCatalog_ProvidesTrustedExecutionMetadata()
    {
        ApprovedInstallationPackage package = BuiltInInstallationCatalog.Create()
            .GetRequired("MICROSOFT.AZURECLI", InstallationSource.Winget);

        Assert.Equal("Azure CLI", package.DisplayName);
        Assert.Equal("devops.azure-cli", package.DiagnosticCheckId);
        Assert.Equal("az", package.VerificationExecutable);
        Assert.Equal(["version"], package.VerificationArguments);
        Assert.True(package.RequiresElevation);
        Assert.True(package.RefreshPathAfterInstall);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static InstallationPlan CreatePlan(params InstallationPlanItem[] items)
    {
        return new InstallationPlan
        {
            PlanId = Guid.NewGuid().ToString("D"),
            Items = items
        };
    }
}
