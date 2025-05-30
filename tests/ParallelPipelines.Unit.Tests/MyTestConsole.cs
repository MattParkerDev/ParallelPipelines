﻿using Spectre.Console;
using Spectre.Console.Rendering;

namespace ParallelPipelines.Unit.Tests;

/// <summary>
/// Used to capture the output of the console to the test output
/// </summary>
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
