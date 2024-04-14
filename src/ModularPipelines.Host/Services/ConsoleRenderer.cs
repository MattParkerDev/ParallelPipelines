using Domain.Entities;
using Domain.Enums;
using Spectre.Console;

namespace ModularPipelines.Host.Services;

public static class ConsoleRenderer
{
	private static bool HasRenderedOnce { get; set; } = false;
	private static int NumberOfModules { get; set; } = 0;
	public static void RenderModulesProgress(List<ModuleContainer> moduleContainers)
	{
		lock ("ConsoleRenderer")
		{
			if (HasRenderedOnce is false)
			{
				AnsiConsole.WriteLine("Module\t\t\t\tStatus");
			}
			if (HasRenderedOnce)
			{
				Console.SetCursorPosition(0, Console.CursorTop - NumberOfModules);
				AnsiConsole.Write("\x1B[0J"); // clear from cursor to end of screen https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#erase-functions
			}
			foreach (var module in moduleContainers)
			{
				var text = GetDecoratedText(module);

				AnsiConsole.WriteLine(text);
			}

			if (!HasRenderedOnce)
			{
				HasRenderedOnce = true;
				NumberOfModules = moduleContainers.Count;
			}
		}
	}

	private static string GetDecoratedText(ModuleContainer module)
	{
		return module.State switch
		{
			ModuleState.Waiting => $"{module.GetModuleName()}\t\tWaiting",
			ModuleState.Running => $"\x1b[36m{module.GetModuleName()}\t\tRunning\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Success => $"\x1b[32m{module.GetModuleName()}\t\tSuccess\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Skipped => $"\x1b[33m{module.GetModuleName()}\t\tSkipped\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Cancelled => $"\x1b[31m{module.GetModuleName()}\t\tCancelled\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Failure => $"\x1b[31m{module.GetModuleName()}\t\tFailure\x1b[0m",
			_ => throw new ArgumentOutOfRangeException(nameof(module.State))
		};
	}
}
