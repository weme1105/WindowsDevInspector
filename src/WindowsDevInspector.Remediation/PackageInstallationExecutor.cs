namespace WindowsDevInspector.Remediation;

public sealed class PackageInstallationExecutor(
    InstallationPlanValidator validator,
    ApprovedInstallationCatalog catalog,
    IPackageInstallationProcessRunner processRunner,
    IPackageInstallationVerifier verifier,
    IProcessPathRefresher pathRefresher,
    PackageInstallationExecutorOptions options)
{
    public async Task<PackageInstallationExecutionResult> ExecuteAsync(
        InstallationPlan? plan,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstallationPlanValidationResult validation = validator.Validate(plan);
        if (!validation.IsValid || plan is null)
        {
            return PackageInstallationExecutionResult.Invalid(plan?.PlanId ?? string.Empty, validation.Errors);
        }

        List<PackageInstallationItemResult> results = [];
        PackageInstallationCommandPreviewBuilder previewBuilder = new(catalog);

        foreach (InstallationPlanItem item in plan.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ApprovedInstallationPackage package = catalog.GetRequired(item.PackageId, item.Source);
            PackageInstallationCommandPreview command = previewBuilder.Build(item);

            if (!options.ExecutionEnabled)
            {
                results.Add(new PackageInstallationItemResult
                {
                    PackageId = package.PackageId,
                    Outcome = PackageInstallationOutcome.Disabled,
                    Message = "Package installation is disabled by executor options.",
                    CommandPreview = command,
                    Verification = PackageInstallationVerificationResult.NotRun(
                        "Verification was not run because package installation is disabled.")
                });
                continue;
            }

            PackageInstallationProcessResult processResult = await processRunner.RunAsync(
                command,
                options.Timeout,
                cancellationToken);
            PackageInstallationVerificationResult verification =
                PackageInstallationVerificationResult.NotRun(
                    "Verification was not run because package installation did not succeed.");

            if (processResult.Outcome == PackageInstallationOutcome.Succeeded)
            {
                if (package.RefreshPathAfterInstall)
                {
                    pathRefresher.Refresh();
                }

                verification = await verifier.VerifyAsync(
                    package,
                    options.VerificationTimeout,
                    cancellationToken);
            }

            results.Add(new PackageInstallationItemResult
            {
                PackageId = package.PackageId,
                Outcome = processResult.Outcome == PackageInstallationOutcome.Succeeded
                    && verification.Outcome != PackageInstallationVerificationOutcome.Succeeded
                        ? PackageInstallationOutcome.Failed
                        : processResult.Outcome,
                Message = verification.Outcome == PackageInstallationVerificationOutcome.Succeeded
                    ? $"{processResult.Message} {verification.Message}"
                    : processResult.Outcome == PackageInstallationOutcome.Succeeded
                        ? $"{processResult.Message} Post-install verification failed: {verification.Message}"
                        : processResult.Message,
                CommandPreview = command,
                Verification = verification
            });
        }

        return new PackageInstallationExecutionResult
        {
            PlanId = plan.PlanId,
            Succeeded = results.Count > 0
                && results.All(result => result.Outcome == PackageInstallationOutcome.Succeeded),
            Errors = [],
            Results = results
        };
    }

}
