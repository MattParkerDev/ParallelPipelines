using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit.Abstractions;

namespace ParallelPipelines.Unit.Tests;

public class MyTestConsole(ITestOutputHelper testOutput) : IAnsiConsole
{
	private readonly ITestOutputHelper _testOutput = testOutput;

	public Profile Profile { get; } = AnsiConsole.Profile;
	public IAnsiConsoleCursor Cursor { get; } = AnsiConsole.Cursor;
	public IAnsiConsoleInput Input { get; } = AnsiConsole.Console.Input;
	public IExclusivityMode ExclusivityMode { get; } = AnsiConsole.Console.ExclusivityMode;
	public RenderPipeline Pipeline { get; } = AnsiConsole.Console.Pipeline;

	public void Clear(bool home)
	{
		throw new NotImplementedException();
	}

	public void Write(IRenderable renderable)
	{
		var allText = renderable.GetSegments(AnsiConsole.Console).Select(x => x.Text);
		var text = string.Join(string.Empty, allText).ReplaceLineEndings(string.Empty);
		_testOutput.WriteLine(text);
	}
}
