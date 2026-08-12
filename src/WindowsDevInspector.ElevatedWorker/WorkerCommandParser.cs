namespace WindowsDevInspector.ElevatedWorker;

public sealed class WorkerCommandParser
{
    public const string Usage =
        "Usage: WindowsDevInspector.ElevatedWorker <change-plan.json> [result.json] OR --rollback <backup.json> [result.json] OR --install <installation-plan.json> [result.json]";

    public WorkerCommandParseResult Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool isRollback = args.Count > 0
            && string.Equals(args[0], "--rollback", StringComparison.OrdinalIgnoreCase);
        bool isInstall = args.Count > 0
            && string.Equals(args[0], "--install", StringComparison.OrdinalIgnoreCase);
        bool hasValidArguments = isRollback || isInstall
            ? args.Count is 2 or 3
            : args.Count is 1 or 2;

        if (!hasValidArguments)
        {
            return WorkerCommandParseResult.Invalid(Usage);
        }

        return WorkerCommandParseResult.Valid(new WorkerCommand
        {
            Mode = isRollback
                ? WorkerCommandMode.Rollback
                : isInstall
                    ? WorkerCommandMode.Installation
                    : WorkerCommandMode.Remediation,
            InputPath = isRollback || isInstall ? args[1] : args[0],
            ResultPath = isRollback || isInstall
                ? args.Count == 3 ? args[2] : null
                : args.Count == 2 ? args[1] : null
        });
    }
}
