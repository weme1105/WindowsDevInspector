namespace WindowsDevInspector.App;

public static class ScanConcurrencySettings
{
    public static int ProcessorCount => Math.Max(1, Environment.ProcessorCount);

    public static int DefaultMaxConcurrentChecks => RecommendedForProcessorCount(ProcessorCount);

    public static IReadOnlyList<int> CreateOptions()
    {
        return Enumerable.Range(1, ProcessorCount).ToArray();
    }

    public static int RecommendedForProcessorCount(int processorCount)
    {
        return Math.Max(1, processorCount - 1);
    }

    public static int Clamp(int value)
    {
        return Math.Clamp(value, 1, ProcessorCount);
    }
}
