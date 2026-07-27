using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public sealed class CheckResultRow(CheckResult result)
{
    public string Severity { get; } = result.Severity.ToString();

    public string Category { get; } = result.Category;

    public string Name { get; } = result.Name;

    public string CurrentValue { get; } = result.CurrentValue;

    public string CanFix { get; } = result.CanFix ? "是" : "否";

    public string Impact { get; } = result.Impact;
}
