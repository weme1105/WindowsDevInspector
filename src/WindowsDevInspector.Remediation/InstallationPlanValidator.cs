namespace WindowsDevInspector.Remediation;

public sealed class InstallationPlanValidator(ApprovedInstallationCatalog catalog)
{
    private const int RequiredItemCount = 1;

    public InstallationPlanValidationResult Validate(InstallationPlan? plan)
    {
        List<string> errors = [];

        if (plan is null)
        {
            return InstallationPlanValidationResult.Invalid(["Installation plan is missing or invalid JSON."]);
        }

        if (!Guid.TryParseExact(plan.PlanId, "D", out _))
        {
            errors.Add("PlanId must be a GUID in D format.");
        }

        if (plan.Items is null)
        {
            errors.Add("Items is required.");
        }
        else if (plan.Items.Count != RequiredItemCount)
        {
            errors.Add("An installation plan must contain exactly one item.");
        }

        if (plan.Items is not null)
        {
            HashSet<(InstallationSource Source, string PackageId)> seenPackages =
                new(new PackageKeyComparer());

            foreach (InstallationPlanItem item in plan.Items)
            {
                if (item is null)
                {
                    errors.Add("Every installation item is required.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.PackageId))
                {
                    errors.Add("PackageId is required for every installation item.");
                    continue;
                }

                if (!Enum.IsDefined(item.Source))
                {
                    errors.Add($"Installation source '{item.Source}' is not supported.");
                    continue;
                }

                if (item.Action != InstallationAction.Install)
                {
                    errors.Add($"Installation action '{item.Action}' is not supported.");
                    continue;
                }

                if (!seenPackages.Add((item.Source, item.PackageId)))
                {
                    errors.Add($"Duplicate package '{item.PackageId}' from source '{item.Source}' is not allowed.");
                    continue;
                }

                if (!catalog.IsApproved(item.PackageId, item.Source))
                {
                    errors.Add($"Package '{item.PackageId}' from source '{item.Source}' is not approved for installation.");
                }
            }
        }

        return errors.Count == 0
            ? InstallationPlanValidationResult.Valid()
            : InstallationPlanValidationResult.Invalid(errors);
    }

    private sealed class PackageKeyComparer : IEqualityComparer<(InstallationSource Source, string PackageId)>
    {
        public bool Equals(
            (InstallationSource Source, string PackageId) x,
            (InstallationSource Source, string PackageId) y)
        {
            return x.Source == y.Source
                && StringComparer.OrdinalIgnoreCase.Equals(x.PackageId, y.PackageId);
        }

        public int GetHashCode((InstallationSource Source, string PackageId) key)
        {
            return HashCode.Combine(
                key.Source,
                StringComparer.OrdinalIgnoreCase.GetHashCode(key.PackageId));
        }
    }
}
