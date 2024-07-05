using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class ConsoleRenderer(IAnsiConsole ansiConsole)
{
	private readonly IAnsiConsole _ansiConsole = ansiConsole;
	private static bool _zeroTimesToFirstStep = true;
	private bool HasRenderedOnce { get; set; } = false;
	private int NumberOfSteps { get; set; } = 0;
	private List<StepContainer>? StepContainers { get; set; }
	public bool StopRendering { get; set; } = false;

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public async Task StartRenderingProgress(List<StepContainer> stepContainers, CancellationToken cancellationToken)
	{
		if (DeploymentConstants.WriteDynamicLogs is false)
		{
			return;
		}
		StepContainers = stepContainers;
		while (cancellationToken.IsCancellationRequested is false && StopRendering is false)
		{
			await RenderStepsProgress(StepContainers);
			await Task.Delay(1000, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
		}
	}
	public async Task RenderStepsProgress(List<StepContainer> stepContainers, PipelineSummary? pipelineSummary = null, bool finalWrite = false)
	{
		if (DeploymentConstants.WriteDynamicLogs is false && finalWrite is false)
		{
			return;
		}
		await semaphoreSlim.WaitAsync(CancellationToken.None);
		try
		{
			if (HasRenderedOnce is false)
			{
				var consoleWidth = AnsiConsole.Profile.Width;
				if (DeploymentConstants.IsGithubActions || consoleWidth < 100)
				{
					_ansiConsole.WriteLine($"Console Width: {consoleWidth}, overriding to 120");
					_ansiConsole.Profile.Width = 120;
				}
				_ansiConsole.WriteLine($"{"Step",-40}{"Status", -14}{"Start", -15}{"End", -15}{"Duration", -11}");
			}
			if (HasRenderedOnce)
			{
				_ansiConsole.Cursor.SetPosition(0, Console.CursorTop - NumberOfSteps - 2 + 1);
				_ansiConsole.Write("\x1B[0J"); // clear from cursor to end of screen https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#erase-functions
			}
			foreach (var step in stepContainers)
			{
				var text = GetDecoratedText(step);
				_ansiConsole.WriteLine(text);
			}

			var (startTime, endTime, duration) = GetTimeStartedAndFinishedGlobal();
			var (start, end) = GetAnsiColorCodes(pipelineSummary?.OverallCompletionType);
			var pipelineStatusString = GetStatusString(pipelineSummary?.OverallCompletionType);

			_ansiConsole.WriteLine("─────────────────────────────────────────────────────────────────────────────────────────────");
			_ansiConsole.WriteLine($"{"Total",-40}{start}{pipelineStatusString, -14}{end}{startTime, -15}{endTime, -15}{duration, -11}");

			if (!HasRenderedOnce)
			{
				HasRenderedOnce = true;
				NumberOfSteps = stepContainers.Count;
			}
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	public void WriteStep(StepContainer stepContainer)
	{
		if (DeploymentConstants.WriteDynamicLogs is true)
		{
			return;
		}
		var text = stepContainer switch
		{
			{ State: StepState.Completed, CompletionType: CompletionType.Success } => $"✅ {stepContainer.GetStepName()} finished Successfully",
			{ State: StepState.Completed, CompletionType: CompletionType.Skipped } => $"{stepContainer.GetStepName()} skipped",
			{ State: StepState.Completed, CompletionType: CompletionType.Cancelled } => $"{stepContainer.GetStepName()} cancelled due to previous failure",
			{ State: StepState.Completed, CompletionType: CompletionType.Failure } => $"❌ {stepContainer.GetStepName()} Failed: {stepContainer.Exception}",
			{ State: StepState.Running } => $"⚡ {stepContainer.GetStepName()} Starting",
			_ => throw new ArgumentOutOfRangeException(nameof(stepContainer))
		};
		_ansiConsole.WriteLine(text);
	}

	public async Task WriteFinalState(PipelineSummary pipelineSummary, List<StepContainer> stepContainers)
	{
		await RenderStepsProgress(stepContainers, pipelineSummary, true);
	}

	private string GetDecoratedText(StepContainer step)
	{
		var (start, end) = GetAnsiColorCodes(step);
		var (startTime, endTime, duration) = GetTimeStartedAndFinished(step);
		var text = $"{start}{step.GetStepName(),-40}{GetStatusString(step), -14}{startTime, -15}{endTime, -15}{duration, -11}{end}";
		return text;
	}



	public static (string start, string end) GetAnsiColorCodes(CompletionType? completionType)
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

	private static (string start, string end) GetAnsiColorCodes(StepContainer step)
	{
		return step.State switch
		{
			StepState.Waiting => ("",""),
			StepState.Running => ("\x1b[36m","\x1b[0m"),
			StepState.Completed when step.CompletionType == CompletionType.Success => ("\x1b[32m","\x1b[0m"),
			StepState.Completed when step.CompletionType == CompletionType.Skipped => ("\x1b[33m","\x1b[0m"),
			StepState.Completed when step.CompletionType == CompletionType.Cancelled => ("\x1b[31m","\x1b[0m"),
			StepState.Completed when step.CompletionType == CompletionType.Failure => ("\x1b[31m","\x1b[0m"),
			_ => throw new ArgumentOutOfRangeException(nameof(step.State))
		};
	}

	public static string GetStatusString(CompletionType? completionType)
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
	public static string GetStatusString(StepContainer step)
	{
		return step.State switch
		{
			StepState.Waiting => "Waiting",
			StepState.Running => "Running",
			StepState.Completed when step.CompletionType == CompletionType.Success => "Success",
			StepState.Completed when step.CompletionType == CompletionType.Skipped => "Skipped",
			StepState.Completed when step.CompletionType == CompletionType.Cancelled => "Cancelled",
			StepState.Completed when step.CompletionType == CompletionType.Failure => $"Failure",
			_ => throw new ArgumentOutOfRangeException(nameof(step.State))
		};
	}

	public static (string? startTime, string? endTime, string? duration) GetTimeStartedAndFinishedGlobal()
	{
		var startTimeGlobal = DeploymentTimeProvider.DeploymentStartTime;
		var endTimeGlobal = DeploymentTimeProvider.DeploymentEndTime;
		var durationGlobal = DeploymentTimeProvider.DeploymentDuration;
		if (_zeroTimesToFirstStep is false)
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

	public static (string? startTime, string? endTime, string? duration) GetTimeStartedAndFinished(StepContainer step)
	{
		if (_zeroTimesToFirstStep is false)
		{
			var startTime = step.StartTime?.ToString("HH:mm:ss");
			var endTime = step.EndTime?.ToString("HH:mm:ss");
			var duration = step.Duration?.ToString(@"hh\:mm\:ss");
			return (startTime, endTime, duration);
		}
		else
		{
			var startTime = step.StartTime - DeploymentTimeProvider.DeploymentStartTime;
			var endTime = step.EndTime - DeploymentTimeProvider.DeploymentStartTime;
			var duration = endTime - startTime;
			if (duration is null)
			{
				duration = DateTimeOffset.Now - step.StartTime;
			}
			return (startTime.ToTimeSpanString(), endTime.ToTimeSpanString(), duration.ToTimeSpanString());
		}
	}
}

public static class ConsoleRendererExtensions
{
	public static string? ToTimeSpanString(this TimeSpan? timeSpan)
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

	public static string GetDecoratedStatusString(this CompletionType? completionType)
	{
		var pipelineStatusString = ConsoleRenderer.GetStatusString(completionType);
		var (start, end) = ConsoleRenderer.GetAnsiColorCodes(completionType);
		return $"{start}{pipelineStatusString}{end}";
	}
}
