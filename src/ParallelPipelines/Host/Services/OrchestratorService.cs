using Microsoft.Extensions.Options;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;
using ParallelPipelines.Application.Attributes;
using ParallelPipelines.Host.Helpers;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class OrchestratorService(StepContainerProvider stepContainerProvider, ConsoleRenderer consoleRenderer, IAnsiConsole ansiConsole, IOptions<PipelineConfig> pipelineConfig)
{
	private readonly StepContainerProvider _stepContainerProvider = stepContainerProvider;
	private readonly ConsoleRenderer _consoleRenderer = consoleRenderer;
	private readonly IAnsiConsole _ansiConsole = ansiConsole;
	private readonly PipelineConfig _pipelineConfig = pipelineConfig.Value;
	private readonly bool _exitPipelineOnSingleFailure = true;
	private bool _isPipelineCancellationRequested = false;
	private bool _runStepsSequentially = false;

	public async Task InitialiseAsync(CancellationToken cancellationToken)
	{
		_ansiConsole.WriteLine("\x1b[36m📦 Starting ParallelPipelines...\x1b[0m");

		await PipelineFileHelper.PopulateGitRootDirectory(cancellationToken);
		DeploymentConstants.IsGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
		DeploymentConstants.ConsoleSupportsAnsiSequences = AnsiConsole.Profile.Capabilities.Ansi;
		DeploymentConstants.WriteDynamicLogs = DeploymentConstants.ConsoleSupportsAnsiSequences && DeploymentConstants.IsGithubActions is false;
	}

	public async Task<PipelineSummary> RunPipeline(CancellationToken cancellationToken)
	{
		var stepContainers = _stepContainerProvider.GetAllStepContainers();
		if (stepContainers.Count == 0)
		{
			throw new InvalidOperationException("ParallelPipelines failed - No steps found to execute");
		}

		LogFoundSteps(stepContainers);
		PopulateDependents(stepContainers);
		PopulateDependencies(stepContainers);

		var stepsToExecute = _stepContainerProvider.GetStepContainersOrderedForExecution(cancellationToken);

		using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		DeploymentTimeProvider.DeploymentStartTime = DateTimeOffset.Now;
		using var consoleRenderingLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		var consoleRenderingTask = _consoleRenderer.StartRenderingProgress(stepContainers, consoleRenderingLinkedCts.Token);
		try
		{
			var parallelOptions = new ParallelOptions
			{
				CancellationToken = linkedCts.Token,
			};
			if (_runStepsSequentially)
			{
				parallelOptions.MaxDegreeOfParallelism = 1;
			}
			await Parallel.ForEachAsync(stepsToExecute, parallelOptions, async (stepContainer, ct) =>
			{
				try
				{
					if (_isPipelineCancellationRequested)
					{
						SetStepState(stepContainer, StepState.Completed, CompletionType.Cancelled);
					}
					else if (stepContainer.Step.ShouldSkip())
					{
						SetStepState(stepContainer, StepState.Completed, CompletionType.Skipped);
					}
					else
					{
						SetStepState(stepContainer, StepState.Running, null, newStepStarting: true, completeAsyncTask: false);
						var results = await stepContainer.Step.RunStep(ct);
						stepContainer.CliCommandResults = results;
						SetStepState(stepContainer, StepState.Completed, CompletionType.Success);
					}
				}
				catch (Exception ex)
				{
					if (ex is TaskCanceledException or OperationCanceledException)
					{
						SetStepState(stepContainer, StepState.Completed, CompletionType.Cancelled, completeAsyncTask: false);
					}
					else
					{
						stepContainer.Exception = ex;
						SetStepState(stepContainer, StepState.Completed, CompletionType.Failure, completeAsyncTask: false);
					}
					await _consoleRenderer.RenderStepsProgress(stepContainers);
					if (_exitPipelineOnSingleFailure)
					{
						_isPipelineCancellationRequested = true;
						await linkedCts.CancelAsync();
					}
					stepContainer.CompletedTask.Start();
				}
			});
		}
		catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
		{
			// Don't throw if await Parallel.ForEachAsync has thrown
		}

		if (_isPipelineCancellationRequested)
		{
			stepContainers.Where(s => s.State == StepState.Waiting).ToList().ForEach(c => SetStepState(c, StepState.Completed, CompletionType.Cancelled));
		}
		DeploymentTimeProvider.DeploymentEndTime = DateTimeOffset.Now;
		var pipelineSummary = GetPipelineSummary(stepContainers);
		_consoleRenderer.StopRendering = true;
		await consoleRenderingLinkedCts.CancelAsync();
		await consoleRenderingTask;
		await _consoleRenderer.WriteFinalState(pipelineSummary, stepContainers);
		_ansiConsole.WriteLine();
		stepContainers.Where(s => s.Exception != null).ToList().ForEach(s => _ansiConsole.WriteLine($"❌ {s.GetStepName()} Failed: {s.Exception}"));
		_ansiConsole.WriteLine($"ParallelPipelines finished - {pipelineSummary.OverallCompletionType.GetDecoratedStatusString()}");
		pipelineSummary.DeploymentStartTime = DeploymentTimeProvider.DeploymentStartTime;
		pipelineSummary.DeploymentEndTime = DeploymentTimeProvider.DeploymentEndTime;
		pipelineSummary.StepContainers = stepContainers;

		return pipelineSummary;
	}

	private PipelineSummary GetPipelineSummary(List<StepContainer> stepContainers)
	{
		var pipelineSummary = new PipelineSummary
		{
			OverallCompletionType = stepContainers switch
			{
				_ when _exitPipelineOnSingleFailure is true && stepContainers.Any(m => m.CompletionType == CompletionType.Failure) => CompletionType.Failure,
				_ when stepContainers.Any(m => m.CompletionType == CompletionType.Cancelled) => CompletionType.Cancelled,
				_ when stepContainers.Any(m => m.CompletionType is CompletionType.Success or CompletionType.Skipped) => CompletionType.Success,
				_ => throw new ArgumentOutOfRangeException(nameof(stepContainers), "Could not determine pipeline completion type")
			}
		};

		return pipelineSummary;
	}

	private void SetStepState(StepContainer stepContainer, StepState state, CompletionType? completionType, bool newStepStarting = false, bool completeAsyncTask = true)
	{
		stepContainer.CompletionType = completionType;
		stepContainer.State = state;
		stepContainer.StartTime ??= DateTimeOffset.Now;
		if (newStepStarting is false)
		{
			stepContainer.HasCompleted = true;
			stepContainer.EndTime ??= DateTimeOffset.Now;
		}
		if (completeAsyncTask is true)
		{
			stepContainer.CompletedTask.Start();
		}
		_consoleRenderer.WriteStep(stepContainer);
	}

	private void LogFoundSteps(List<StepContainer> stepContainers)
	{
		_ansiConsole.WriteLine($"Found {stepContainers.Count} step/s");
		_ansiConsole.WriteLine();
		if (DeploymentConstants.WriteDynamicLogs is false)
		{
			stepContainers.ForEach(c => _ansiConsole.WriteLine($"⭐ Found {c.Step.GetType().Name}"));
			_ansiConsole.WriteLine();
		}
	}

	private static void PopulateDependents(List<StepContainer> stepContainers)
	{
		foreach (var stepContainer in stepContainers)
		{
			var dependencyTypes = stepContainer.Step.GetType().GetDependencyTypes();
			var dependencies = stepContainers.Where(m => dependencyTypes.Contains(m.Step.GetType()));
			foreach (var dependency in dependencies)
			{
				dependency.Dependents.Add(stepContainer);
			}
		}
	}

	private static void PopulateDependencies(List<StepContainer> stepContainers)
	{
		foreach (var stepContainer in stepContainers)
		{
			stepContainer.Dependents.ForEach(d => d.Dependencies.Add(stepContainer));
		}
	}
}
