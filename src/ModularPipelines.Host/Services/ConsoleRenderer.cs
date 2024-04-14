using Domain.Entities;
using Domain.Enums;
using ModularPipelines.Host.InternalHelpers;
using Spectre.Console;

namespace ModularPipelines.Host.Services;

public static class ConsoleRenderer
{
	public static bool ZeroTimesToFirstModule = true;
	private static bool HasRenderedOnce { get; set; } = false;
	private static int NumberOfModules { get; set; } = 0;
	public static void RenderModulesProgress(List<ModuleContainer> moduleContainers)
	{
		lock ("ConsoleRenderer")
		{
			if (HasRenderedOnce is false)
			{
				AnsiConsole.WriteLine($"{"Module",-40}{"Status", -20}{"Start", -20}{"End", -20}{"Duration", -20}");
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
		var (start, end) = GetAnsiColorCodes(module);
		var (startTime, endTime, duration) = GetTimeStartedAndFinished(module);
		var text = $"{start}{module.GetModuleName(),-40}{GetStatusString(module), -20}{startTime, -20}{endTime, -20}{duration, -20}{end}";
		return text;
	}

	private static (string start, string end) GetAnsiColorCodes(ModuleContainer module)
	{
		return module.State switch
		{
			ModuleState.Waiting => ("",""),
			ModuleState.Running => ("\x1b[36m","\x1b[0m"),
			ModuleState.Completed when module.CompletionType == CompletionType.Success => ("\x1b[32m","\x1b[0m"),
			ModuleState.Completed when module.CompletionType == CompletionType.Skipped => ("\x1b[33m","\x1b[0m"),
			ModuleState.Completed when module.CompletionType == CompletionType.Cancelled => ("\x1b[31m","\x1b[0m"),
			ModuleState.Completed when module.CompletionType == CompletionType.Failure => ("\x1b[31m","\x1b[0m"),
			_ => throw new ArgumentOutOfRangeException(nameof(module.State))
		};
	}
	private static string GetStatusString(ModuleContainer module)
	{
		return module.State switch
		{
			ModuleState.Waiting => "Waiting",
			ModuleState.Running => "Running",
			ModuleState.Completed when module.CompletionType == CompletionType.Success => "Success",
			ModuleState.Completed when module.CompletionType == CompletionType.Skipped => "Skipped",
			ModuleState.Completed when module.CompletionType == CompletionType.Cancelled => "Cancelled",
			ModuleState.Completed when module.CompletionType == CompletionType.Failure => $"Failure",
			_ => throw new ArgumentOutOfRangeException(nameof(module.State))
		};
	}

	private static (string? startTime, string? endTime, string? duration) GetTimeStartedAndFinished(ModuleContainer module)
	{
		if (ZeroTimesToFirstModule is false)
		{
			var startTime = module.StartTime?.ToString("HH:mm:ss");
			var endTime = module.EndTime?.ToString("HH:mm:ss");
			var duration = module.Duration?.ToString(@"hh\:mm\:ss");
			return (startTime, endTime, duration);
		}
		else
		{
			var startTime = module.StartTime - DeploymentTimeProvider.DeploymentStartTime;
			var endTime = module.EndTime - DeploymentTimeProvider.DeploymentStartTime;
			var duration = endTime - startTime;
			return (startTime.ToTimeSpanString(), endTime.ToTimeSpanString(), duration.ToTimeSpanString());
		}
	}
	private static string? ToTimeSpanString(this TimeSpan? timeSpan)
	{
		return timeSpan?.ToString(@"mm\m\:ss\s\:ff\m\s");
	}
}
