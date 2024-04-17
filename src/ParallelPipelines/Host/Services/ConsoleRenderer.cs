using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public static class ConsoleRenderer
{
	public static bool ZeroTimesToFirstModule = true;
	private static bool HasRenderedOnce { get; set; } = false;
	private static int NumberOfModules { get; set; } = 0;
	private static List<ModuleContainer>? ModuleContainers { get; set; }

	public static async Task StartRenderingProgress(List<ModuleContainer> moduleContainers, CancellationToken cancellationToken)
	{
		if (DeploymentConstants.IsGithubActions || DeploymentConstants.ConsoleSupportsAnsiSequences is false)
		{
			return;
		}
		ModuleContainers = moduleContainers;
		while (!cancellationToken.IsCancellationRequested)
		{
			RenderModulesProgress(ModuleContainers);
			await Task.Delay(1000, cancellationToken);
		}
	}
	public static void RenderModulesProgress(List<ModuleContainer> moduleContainers, PipelineSummary? pipelineSummary = null, bool finalWrite = false)
	{
		if ((DeploymentConstants.IsGithubActions || DeploymentConstants.ConsoleSupportsAnsiSequences is false) && finalWrite is false)
		{
			return;
		}
		lock ("ConsoleRenderer")
		{
			if (HasRenderedOnce is false)
			{
				if (DeploymentConstants.IsGithubActions)
				{
					var consoleWidth = AnsiConsole.Profile.Width;
					AnsiConsole.WriteLine($"Console Width: {consoleWidth}, overriding to 120");
					AnsiConsole.Profile.Width = 120;
				}
				AnsiConsole.WriteLine($"{"Module",-40}{"Status", -14}{"Start", -15}{"End", -15}{"Duration", -11}");
			}
			if (HasRenderedOnce)
			{
				AnsiConsole.Console.Cursor.SetPosition(0, Console.CursorTop - NumberOfModules - 2 + 1);
				AnsiConsole.Write("\x1B[0J"); // clear from cursor to end of screen https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#erase-functions
			}
			foreach (var module in moduleContainers)
			{
				var text = GetDecoratedText(module);
				AnsiConsole.WriteLine(text);
			}

			var (startTime, endTime, duration) = GetTimeStartedAndFinishedGlobal();
			var (start, end) = GetAnsiColorCodes(pipelineSummary?.OverallCompletionType);
			var pipelineStatusString = GetStatusString(pipelineSummary?.OverallCompletionType);

			AnsiConsole.WriteLine("─────────────────────────────────────────────────────────────────────────────────────────────");
			AnsiConsole.WriteLine($"{"Total",-40}{start}{pipelineStatusString, -14}{end}{startTime, -15}{endTime, -15}{duration, -11}");

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
		var text = $"{start}{module.GetModuleName(),-40}{GetStatusString(module), -14}{startTime, -15}{endTime, -15}{duration, -11}{end}";
		return text;
	}

	public static string GetDecoratedStatusString(this CompletionType? completionType)
	{
		var pipelineStatusString = GetStatusString(completionType);
		var (start, end) = GetAnsiColorCodes(completionType);
		return $"{start}{pipelineStatusString}{end}";
	}

	private static (string start, string end) GetAnsiColorCodes(CompletionType? completionType)
	{
		return completionType switch
		{
			null => ("",""),
			CompletionType.Success => ("\x1b[32m","\x1b[0m"),
			CompletionType.Skipped => ("\x1b[33m","\x1b[0m"),
			CompletionType.Cancelled => ("\x1b[31m","\x1b[0m"),
			CompletionType.Failure => ("\x1b[31m","\x1b[0m"),
			_ => throw new ArgumentOutOfRangeException(nameof(completionType))
		};
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

	private static string GetStatusString(CompletionType? completionType)
	{
		var pipelineStatusString = completionType switch
		{
			null => "Running",
			CompletionType.Success => "Success",
			CompletionType.Failure => "Failure",
			CompletionType.Cancelled => "Cancelled",
			CompletionType.Skipped => throw new InvalidOperationException("Overall pipeline completion type cannot be skipped"),
			_ => throw new ArgumentOutOfRangeException(nameof(completionType))
		};
		return pipelineStatusString;
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

	private static (string? startTime, string? endTime, string? duration) GetTimeStartedAndFinishedGlobal()
	{
		var startTimeGlobal = DeploymentTimeProvider.DeploymentStartTime;
		var endTimeGlobal = DeploymentTimeProvider.DeploymentEndTime;
		var durationGlobal = DeploymentTimeProvider.DeploymentDuration;
		if (ZeroTimesToFirstModule is false)
		{
			var startTime = startTimeGlobal?.ToString("HH:mm:ss");
			var endTime = endTimeGlobal?.ToString("HH:mm:ss");
			var duration = durationGlobal?.ToString(@"hh\:mm\:ss");
			return (startTime, endTime, duration);
		}
		else
		{
			TimeSpan? startTime = TimeSpan.Zero;
			var endTime = endTimeGlobal - startTimeGlobal;
			var duration = endTime - startTime;
			if (duration is null)
			{
				duration = DateTimeOffset.Now - startTimeGlobal;
			}
			return (startTime.ToTimeSpanString(), endTime.ToTimeSpanString(), duration.ToTimeSpanString());
		}
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
			if (duration is null)
			{
				duration = DateTimeOffset.Now - module.StartTime;
			}
			return (startTime.ToTimeSpanString(), endTime.ToTimeSpanString(), duration.ToTimeSpanString());
		}
	}
	private static string? ToTimeSpanString(this TimeSpan? timeSpan)
	{
		if (timeSpan is null)
		{
			return null;
		}
		if (timeSpan.Value.TotalHours >= 1)
		{
			return timeSpan.Value.ToString(@"HH\h\:mm\m\:ss\s");
		}
		if (timeSpan.Value.TotalMinutes >= 1)
		{
			return timeSpan.Value.ToString(@"mm\m\:ss\s");
		}
		return timeSpan.Value.ToString(@"ss\s\:fff\m\s");
		//return timeSpan?.ToString(@"mm\m\:ss\s\:fff\m\s");
	}

	public static void WriteModule(ModuleContainer moduleContainer)
	{
		if (DeploymentConstants.IsGithubActions || DeploymentConstants.ConsoleSupportsAnsiSequences is false)
		{
			return;
		}
		var text = moduleContainer switch
		{
			{ State: ModuleState.Completed, CompletionType: CompletionType.Success } => $"✅ {moduleContainer.GetModuleName()} finished Successfully",
			{ State: ModuleState.Completed, CompletionType: CompletionType.Skipped } => $"{moduleContainer.GetModuleName()} skipped",
			{ State: ModuleState.Completed, CompletionType: CompletionType.Cancelled } => $"{moduleContainer.GetModuleName()} cancelled due to previous failure",
			{ State: ModuleState.Completed, CompletionType: CompletionType.Failure } => $"❌ {moduleContainer.GetModuleName()} Failed: {moduleContainer.Exception}",
			{ State: ModuleState.Running } => $"⚡ {moduleContainer.GetModuleName()} Starting",
			_ => throw new ArgumentOutOfRangeException(nameof(moduleContainer))
		};
		AnsiConsole.WriteLine(text);
	}

	public static void WriteFinalState(PipelineSummary pipelineSummary, List<ModuleContainer> moduleContainers)
	{
		RenderModulesProgress(moduleContainers, pipelineSummary, true);
	}
}
