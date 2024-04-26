using Microsoft.Extensions.Options;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;
using ParallelPipelines.Application.Attributes;
using ParallelPipelines.Host.Helpers;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class OrchestratorService(ModuleContainerProvider moduleContainerProvider, ConsoleRenderer consoleRenderer, IAnsiConsole ansiConsole, IOptions<PipelineConfig> pipelineConfig)
{
	private readonly ModuleContainerProvider _moduleContainerProvider = moduleContainerProvider;
	private readonly ConsoleRenderer _consoleRenderer = consoleRenderer;
	private readonly IAnsiConsole _ansiConsole = ansiConsole;
	private readonly PipelineConfig _pipelineConfig = pipelineConfig.Value;
	private readonly bool _exitPipelineOnSingleFailure = true;
	private bool _isPipelineCancellationRequested = false;
	private bool _runModulesSequentially = false;

	public async Task InitialiseAsync()
	{
		_ansiConsole.WriteLine("\x1b[36m📦 Starting ParallelPipelines...\x1b[0m");

		await PipelineFileHelper.PopulateGitRootDirectory();
		DeploymentConstants.IsGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
		DeploymentConstants.ConsoleSupportsAnsiSequences = AnsiConsole.Profile.Capabilities.Ansi;
		DeploymentConstants.WriteDynamicLogs = DeploymentConstants.ConsoleSupportsAnsiSequences && DeploymentConstants.IsGithubActions is false;
	}

	public async Task<PipelineSummary> RunPipeline(CancellationToken cancellationToken)
	{
		var moduleContainers = _moduleContainerProvider.GetAllModuleContainers();
		if (moduleContainers.Count == 0)
		{
			throw new InvalidOperationException("ParallelPipelines failed - No modules found to execute");
		}

		LogFoundModules(moduleContainers);
		PopulateDependents(moduleContainers);
		PopulateDependencies(moduleContainers);

		var modulesToExecute = _moduleContainerProvider.GetModuleContainersOrderedForExecution(cancellationToken);

		using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		DeploymentTimeProvider.DeploymentStartTime = DateTimeOffset.Now;
		using var consoleRenderingLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		var consoleRenderingTask = _consoleRenderer.StartRenderingProgress(moduleContainers, consoleRenderingLinkedCts.Token);
		try
		{
			var parallelOptions = new ParallelOptions
			{
				CancellationToken = linkedCts.Token,
			};
			if (_runModulesSequentially)
			{
				parallelOptions.MaxDegreeOfParallelism = 1;
			}
			await Parallel.ForEachAsync(modulesToExecute, parallelOptions, async (moduleContainer, ct) =>
			{
				try
				{
					if (_isPipelineCancellationRequested)
					{
						SetModuleState(moduleContainer, ModuleState.Completed, CompletionType.Cancelled);
					}
					else if (moduleContainer.Module.ShouldSkip())
					{
						SetModuleState(moduleContainer, ModuleState.Completed, CompletionType.Skipped);
					}
					else
					{
						SetModuleState(moduleContainer, ModuleState.Running, null, newModuleStarting: true, completeAsyncTask: false);
						var results = await moduleContainer.Module.RunModule(ct);
						moduleContainer.CliCommandResults = results;
						SetModuleState(moduleContainer, ModuleState.Completed, CompletionType.Success);
					}
				}
				catch (Exception ex)
				{
					if (ex is TaskCanceledException or OperationCanceledException)
					{
						SetModuleState(moduleContainer, ModuleState.Completed, CompletionType.Cancelled, completeAsyncTask: false);
					}
					else
					{
						moduleContainer.Exception = ex;
						SetModuleState(moduleContainer, ModuleState.Completed, CompletionType.Failure, completeAsyncTask: false);
					}
					await _consoleRenderer.RenderModulesProgress(moduleContainers);
					if (_exitPipelineOnSingleFailure)
					{
						_isPipelineCancellationRequested = true;
						await linkedCts.CancelAsync();
					}
					moduleContainer.CompletedTask.Start();
				}
			});
		}
		catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
		{
			// Don't throw if await Parallel.ForEachAsync has thrown
		}

		if (_isPipelineCancellationRequested)
		{
			moduleContainers.Where(s => s.State == ModuleState.Waiting).ToList().ForEach(c => SetModuleState(c, ModuleState.Completed, CompletionType.Cancelled));
		}
		DeploymentTimeProvider.DeploymentEndTime = DateTimeOffset.Now;
		var pipelineSummary = GetPipelineSummary(moduleContainers);
		_consoleRenderer.StopRendering = true;
		await consoleRenderingLinkedCts.CancelAsync();
		await consoleRenderingTask;
		await _consoleRenderer.WriteFinalState(pipelineSummary, moduleContainers);
		_ansiConsole.WriteLine();
		moduleContainers.Where(s => s.Exception != null).ToList().ForEach(s => _ansiConsole.WriteLine($"❌ {s.GetModuleName()} Failed: {s.Exception}"));
		_ansiConsole.WriteLine($"ParallelPipelines finished - {pipelineSummary.OverallCompletionType.GetDecoratedStatusString()}");
		pipelineSummary.DeploymentStartTime = DeploymentTimeProvider.DeploymentStartTime;
		pipelineSummary.DeploymentEndTime = DeploymentTimeProvider.DeploymentEndTime;
		pipelineSummary.ModuleContainers = moduleContainers;

		return pipelineSummary;
	}

	private PipelineSummary GetPipelineSummary(List<ModuleContainer> moduleContainers)
	{
		var pipelineSummary = new PipelineSummary
		{
			OverallCompletionType = moduleContainers switch
			{
				_ when _exitPipelineOnSingleFailure is true && moduleContainers.Any(m => m.CompletionType == CompletionType.Failure) => CompletionType.Failure,
				_ when moduleContainers.Any(m => m.CompletionType == CompletionType.Cancelled) => CompletionType.Cancelled,
				_ when moduleContainers.Any(m => m.CompletionType is CompletionType.Success or CompletionType.Skipped) => CompletionType.Success,
				_ => throw new ArgumentOutOfRangeException(nameof(moduleContainers), "Could not determine pipeline completion type")
			}
		};

		return pipelineSummary;
	}

	private void SetModuleState(ModuleContainer moduleContainer, ModuleState state, CompletionType? completionType, bool newModuleStarting = false, bool completeAsyncTask = true)
	{
		moduleContainer.CompletionType = completionType;
		moduleContainer.State = state;
		moduleContainer.StartTime ??= DateTimeOffset.Now;
		if (newModuleStarting is false)
		{
			moduleContainer.HasCompleted = true;
			moduleContainer.EndTime ??= DateTimeOffset.Now;
		}
		if (completeAsyncTask is true)
		{
			moduleContainer.CompletedTask.Start();
		}
		_consoleRenderer.WriteModule(moduleContainer);
	}

	private void LogFoundModules(List<ModuleContainer> moduleContainers)
	{
		_ansiConsole.WriteLine($"Found {moduleContainers.Count} modules");
		_ansiConsole.WriteLine();
		if (DeploymentConstants.WriteDynamicLogs is false)
		{
			moduleContainers.ForEach(c => _ansiConsole.WriteLine($"⭐ Found {c.Module.GetType().Name}"));
			_ansiConsole.WriteLine();
		}
	}

	private static void PopulateDependents(List<ModuleContainer> moduleContainers)
	{
		foreach (var moduleContainer in moduleContainers)
		{
			var dependencyTypes = moduleContainer.Module.GetType().GetDependencyTypes();
			var dependencies = moduleContainers.Where(m => dependencyTypes.Contains(m.Module.GetType()));
			foreach (var dependency in dependencies)
			{
				dependency.Dependents.Add(moduleContainer);
			}
		}
	}

	private static void PopulateDependencies(List<ModuleContainer> moduleContainers)
	{
		foreach (var moduleContainer in moduleContainers)
		{
			moduleContainer.Dependents.ForEach(d => d.Dependencies.Add(moduleContainer));
		}
	}
}
