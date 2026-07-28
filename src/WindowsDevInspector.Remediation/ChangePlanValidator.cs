namespace WindowsDevInspector.Remediation;

public sealed class ChangePlanValidator(RemediationWhitelist whitelist)
{
    private const int MaxItems = 20;

    public ChangePlanValidationResult Validate(ChangePlan? plan, bool allowElevationRequiredItems)
    {
        List<string> errors = [];

        if (plan is null)
        {
            return ChangePlanValidationResult.Invalid(["Change plan is missing or invalid JSON."]);
        }

        if (string.IsNullOrWhiteSpace(plan.PlanId))
        {
            errors.Add("PlanId is required.");
        }

        if (plan.Items is null)
        {
            errors.Add("Items is required.");
        }
        else if (plan.Items.Count == 0)
        {
            errors.Add("At least one remediation item is required.");
        }
        else if (plan.Items.Count > MaxItems)
        {
            errors.Add($"A change plan cannot contain more than {MaxItems} items.");
        }

        if (plan.Items is not null)
        {
            HashSet<string> seenIds = new(StringComparer.OrdinalIgnoreCase);

            foreach (ChangePlanItem item in plan.Items)
            {
                if (string.IsNullOrWhiteSpace(item.RemediationId))
                {
                    errors.Add("RemediationId is required for every item.");
                    continue;
                }

                if (!seenIds.Add(item.RemediationId))
                {
                    errors.Add($"Duplicate remediation '{item.RemediationId}' is not allowed.");
                    continue;
                }

                RemediationDefinition definition;
                try
                {
                    definition = whitelist.GetRequired(item.RemediationId);
                }
                catch (InvalidOperationException)
                {
                    errors.Add($"Remediation '{item.RemediationId}' is not whitelisted.");
                    continue;
                }

                if (definition.RequiresElevation && !allowElevationRequiredItems)
                {
                    errors.Add($"Remediation '{item.RemediationId}' requires elevation.");
                }
            }
        }

        return errors.Count == 0
            ? ChangePlanValidationResult.Valid()
            : ChangePlanValidationResult.Invalid(errors);
    }
}
