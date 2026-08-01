namespace WindowsDevInspector.Windows;

internal static class StringLineExtensions
{
    public static string[] SplitLines(this string value)
    {
        return value.Split(
            ["\r\n", "\n"],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
