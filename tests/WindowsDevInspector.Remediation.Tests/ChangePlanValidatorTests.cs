namespace WindowsDevInspector.Remediation.Tests;

public sealed class ChangePlanValidatorTests
{
    [Fact]
    public void Validate_AcceptsWhitelistedLocalRemediation()
    {
        ChangePlanValidator validator = new(BuiltInRemediationCatalog.CreateWhitelist());
        ChangePlan plan = new()
        {
            PlanId = "plan-1",
            Items = [new ChangePlanItem { RemediationId = "create-source-directory" }]
        };

        ChangePlanValidationResult result = validator.Validate(plan, allowElevationRequiredItems: false);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsUnknownRemediation()
    {
        ChangePlanValidator validator = new(BuiltInRemediationCatalog.CreateWhitelist());
        ChangePlan plan = new()
        {
            PlanId = "plan-1",
            Items = [new ChangePlanItem { RemediationId = "run-arbitrary-command" }]
        };

        ChangePlanValidationResult result = validator.Validate(plan, allowElevationRequiredItems: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("not whitelisted", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsElevationRequiredItemsWhenElevationIsNotAllowed()
    {
        ChangePlanValidator validator = new(BuiltInRemediationCatalog.CreateWhitelist());
        ChangePlan plan = new()
        {
            PlanId = "plan-1",
            Items = [new ChangePlanItem { RemediationId = "enable-long-paths" }]
        };

        ChangePlanValidationResult result = validator.Validate(plan, allowElevationRequiredItems: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("requires elevation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_AcceptsElevationRequiredItemsWhenElevationIsAllowed()
    {
        ChangePlanValidator validator = new(BuiltInRemediationCatalog.CreateWhitelist());
        ChangePlan plan = new()
        {
            PlanId = "plan-1",
            Items = [new ChangePlanItem { RemediationId = "enable-long-paths" }]
        };

        ChangePlanValidationResult result = validator.Validate(plan, allowElevationRequiredItems: true);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsDuplicateItems()
    {
        ChangePlanValidator validator = new(BuiltInRemediationCatalog.CreateWhitelist());
        ChangePlan plan = new()
        {
            PlanId = "plan-1",
            Items =
            [
                new ChangePlanItem { RemediationId = "create-source-directory" },
                new ChangePlanItem { RemediationId = "CREATE-SOURCE-DIRECTORY" }
            ]
        };

        ChangePlanValidationResult result = validator.Validate(plan, allowElevationRequiredItems: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));
    }
}
