using WindowsDevInspector.ElevatedWorker;

namespace WindowsDevInspector.ElevatedWorker.Tests;

public sealed class WorkerCommandParserTests
{
    private readonly WorkerCommandParser parser = new();

    [Theory]
    [InlineData("plan.json", null)]
    [InlineData("plan.json", "result.json")]
    public void Parse_PreservesRemediationContract(string input, string? result)
    {
        string[] args = result is null ? [input] : [input, result];

        WorkerCommandParseResult parsed = parser.Parse(args);

        Assert.True(parsed.IsValid);
        Assert.Equal(WorkerCommandMode.Remediation, parsed.Command!.Mode);
        Assert.Equal(input, parsed.Command.InputPath);
        Assert.Equal(result, parsed.Command.ResultPath);
    }

    [Theory]
    [InlineData("--rollback", WorkerCommandMode.Rollback)]
    [InlineData("--ROLLBACK", WorkerCommandMode.Rollback)]
    [InlineData("--install", WorkerCommandMode.Installation)]
    [InlineData("--INSTALL", WorkerCommandMode.Installation)]
    public void Parse_PreservesTypedModeContract(string option, WorkerCommandMode expectedMode)
    {
        WorkerCommandParseResult parsed = parser.Parse([option, "input.json", "result.json"]);

        Assert.True(parsed.IsValid);
        Assert.Equal(expectedMode, parsed.Command!.Mode);
        Assert.Equal("input.json", parsed.Command.InputPath);
        Assert.Equal("result.json", parsed.Command.ResultPath);
    }

    [Theory]
    [InlineData()]
    [InlineData("--rollback")]
    [InlineData("--install")]
    [InlineData("--rollback", "input", "result", "extra")]
    [InlineData("--install", "input", "result", "extra")]
    public void Parse_RejectsInvalidArgumentCounts(params string[] args)
    {
        WorkerCommandParseResult parsed = parser.Parse(args);

        Assert.False(parsed.IsValid);
        Assert.Equal(WorkerCommandParser.Usage, parsed.Error);
    }

    [Fact]
    public void Parse_PreservesUnknownSwitchAsRemediationPlanPath()
    {
        WorkerCommandParseResult parsed = parser.Parse(["--unknown"]);

        Assert.True(parsed.IsValid);
        Assert.Equal(WorkerCommandMode.Remediation, parsed.Command!.Mode);
        Assert.Equal("--unknown", parsed.Command.InputPath);
    }
}
