using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public static class RuntimePrerequisiteSelection
{
    public const string RuntimeCheckId = "desktop.dotnet-desktop-runtime";
    public const string BlockReason = "需要 .NET 10 Desktop Runtime x64；Runtime 未通過檢查，因此此操作已停用。";

    public static bool Apply(IEnumerable<CheckResultRow> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        CheckResultRow[] rows = results.ToArray();
        bool runtimeReady = rows.Any(row =>
            row.Id.Equals(RuntimeCheckId, StringComparison.OrdinalIgnoreCase)
            && row.Severity.Equals(CheckSeverity.Pass.ToString(), StringComparison.OrdinalIgnoreCase));

        foreach (CheckResultRow row in rows.Where(row => row.IsActionSelectable))
        {
            row.SetActionBlockReason(runtimeReady ? null : BlockReason);
        }

        return runtimeReady;
    }
}
