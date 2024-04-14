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
			if (HasRenderedOnce)
			{
				Console.SetCursorPosition(0, Console.CursorTop - NumberOfModules);
				AnsiConsole.Write("\x1B[0J"); // clear from cursor to end of screen https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#erase-functions
			}

			foreach (var module in moduleContainers)
			{
				var moduleName = module.Module.GetType().Name;
				var status = GetModuleStatus(module);

				Console.WriteLine($"Module: {moduleName}\t\t {status}");
			}

			if (!HasRenderedOnce)
			{
				HasRenderedOnce = true;
				NumberOfModules = moduleContainers.Count;
			}
		}
	}

	private static string GetModuleStatus(ModuleContainer module)
	{
		return module.State switch
		{
			ModuleState.Waiting => "Waiting...",
			ModuleState.Running => "\x1b[36mRunning...\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Success => "\x1b[32mSuccess\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Skipped => "\x1b[33mSkipped\x1b[0m",
			ModuleState.Completed when module.CompletionType == CompletionType.Cancelled => "\x1b[31mCancelled\x1b[0m",
			_ => throw new ArgumentOutOfRangeException(nameof(module.State))
		};
	}
}
