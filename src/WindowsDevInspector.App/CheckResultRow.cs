using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public sealed class CheckResultRow(CheckResult result)
{
    private const int MaxCellLength = 160;

    public string Id { get; } = result.Id;

    public string Severity { get; } = result.Severity.ToString();

    public string Category { get; } = result.Category;

    public string Name { get; } = result.Name;

    public string CurrentValue { get; } = ToSingleLine(result.CurrentValue);

    public string FullCurrentValue { get; } = result.CurrentValue;

    public string ExpectedValue { get; } = result.ExpectedValue;

    public string CanFix { get; } = result.CanFix ? "是" : "否";

    public string Impact { get; } = result.Impact;

    private static string ToSingleLine(string value)
    {
        string singleLine = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        return singleLine.Length <= MaxCellLength
            ? singleLine
            : string.Concat(singleLine.AsSpan(0, MaxCellLength), "...");
    }
}
