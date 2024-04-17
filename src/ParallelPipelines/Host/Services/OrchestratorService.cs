using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;
using ParallelPipelines.Application.Attributes;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class OrchestratorService(ModuleContainerProvider moduleContainerProvider)
{
	private readonly ModuleContainerProvider _moduleContainerProvider = moduleContainerProvider;
	private readonly bool _exitPipelineOnSingleFailure = true;
	private bool _isPipelineCancellationRequested = false;

	public async Task<PipelineSummary?> RunPipeline(CancellationToken cancellationToken)
	{
		var moduleContainers = _moduleContainerProvider.GetAllModuleContainers();
		if (moduleContainers.Count == 0)
		{
			AnsiConsole.WriteLine("ParallelPipelines failed - No modules found to execute");
			return null;
		}

		LogFoundModules(moduleContainers);
		PopulateDependents(moduleContainers);
		PopulateDependencies(moduleContainers);

		var modulesToExecute = _moduleContainerProvider.GetModuleContainersOrderedForExecution(cancellationToken);

		using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		DeploymentTimeProvider.DeploymentStartTime = DateTimeOffset.Now;
		_ = ConsoleRenderer.StartRenderingProgress(moduleContainers, linkedCts.Token);
		try
		{
			await Parallel.ForEachAsync(modulesToExecute, linkedCts.Token, async (moduleContainer, ct) =>
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
					ConsoleRenderer.RenderModulesProgress(moduleContainers);
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
		ConsoleRenderer.WriteFinalState(pipelineSummary, moduleContainers);
		AnsiConsole.WriteLine();
		moduleContainers.Where(s => s.Exception != null).ToList().ForEach(s => AnsiConsole.WriteLine($"❌ {s.GetModuleName()} Failed: {s.Exception}"));
		AnsiConsole.WriteLine($"ParallelPipelines finished - {pipelineSummary.OverallCompletionType.GetDecoratedStatusString()}");
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
		ConsoleRenderer.WriteModule(moduleContainer);
	}

	private void LogFoundModules(List<ModuleContainer> moduleContainers)
	{
		AnsiConsole.WriteLine($"Found {moduleContainers.Count} modules");
		AnsiConsole.WriteLine();
		if (DeploymentConstants.IsGithubActions || DeploymentConstants.ConsoleSupportsAnsiSequences is false)
		{
			moduleContainers.ForEach(c => AnsiConsole.WriteLine($"⭐ Found {c.Module.GetType().Name}"));
			AnsiConsole.WriteLine();
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
