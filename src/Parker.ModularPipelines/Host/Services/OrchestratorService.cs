using Parker.ModularPipelines.Application.Attributes;
using Parker.ModularPipelines.Domain.Entities;
using Parker.ModularPipelines.Domain.Enums;
using Parker.ModularPipelines.Host.InternalHelpers;
using Spectre.Console;

namespace Parker.ModularPipelines.Host.Services;

public class OrchestratorService(ModuleContainerProvider moduleContainerProvider)
{
	private readonly ModuleContainerProvider _moduleContainerProvider = moduleContainerProvider;
	private readonly bool _exitPipelineOnSingleFailure = true;
	private bool _isPipelineCancellationRequested = false;

	public async Task RunPipeline(CancellationToken cancellationToken)
	{
		AnsiConsole.WriteLine("🚀Executing OrchestratorService");
		var moduleContainers = _moduleContainerProvider.GetAllModuleContainers();

		try
		{
			LogFoundModules(moduleContainers);
			PopulateDependents(moduleContainers);
			PopulateDependencies(moduleContainers);

			var modulesToExecute = _moduleContainerProvider.GetModuleContainersOrderedForExecution(cancellationToken);

			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			DeploymentTimeProvider.DeploymentStartTime = DateTimeOffset.Now;
			_ = ConsoleRenderer.StartRenderingProgress(moduleContainers, cancellationToken);
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
						await moduleContainer.Module.RunModule(ct);
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
						moduleContainer.ExceptionMessage = ex.Message + ex.InnerException?.Message;
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
		finally
		{
			if (_isPipelineCancellationRequested)
			{
				moduleContainers.Where(s => s.State == ModuleState.Waiting).ToList().ForEach(c => SetModuleState(c, ModuleState.Completed, CompletionType.Cancelled));
			}
			ConsoleRenderer.WriteFinalState(moduleContainers);
			AnsiConsole.WriteLine();
			moduleContainers.Where(s => s.ExceptionMessage != null).ToList().ForEach(s => AnsiConsole.WriteLine($"❌ {s.Module.GetType().Name} Failed: {s.ExceptionMessage}"));
			AnsiConsole.WriteLine($"OrchestratorService {(_isPipelineCancellationRequested ? "cancelled" : "completed")}");
			DeploymentTimeProvider.DeploymentEndTime = DateTimeOffset.Now;
		}
	}

	private void SetModuleState(ModuleContainer moduleContainer, ModuleState state, CompletionType? completionType, bool newModuleStarting = false, bool completeAsyncTask = true)
	{
		moduleContainer.HasCompleted = true;
		moduleContainer.CompletionType = completionType;
		moduleContainer.State = state;
		moduleContainer.StartTime ??= DateTimeOffset.Now;
		if (newModuleStarting is false)
		{
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
		if (DeploymentConstants.IsGithubActions)
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
