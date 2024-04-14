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
						moduleContainer.HasCompleted = true;
						moduleContainer.CompletionType = CompletionType.Cancelled;
						moduleContainer.State = ModuleState.Completed;
						moduleContainer.StartTime = DateTimeOffset.Now;
						moduleContainer.EndTime = DateTimeOffset.Now;
						moduleContainer.CompletedTask.Start();
						ConsoleRenderer.WriteCancelledModule(moduleContainer);
					}
					else if (moduleContainer.Module.ShouldSkip())
					{
						moduleContainer.HasCompleted = true;
						moduleContainer.CompletionType = CompletionType.Skipped;
						moduleContainer.State = ModuleState.Completed;
						moduleContainer.StartTime = DateTimeOffset.Now;
						moduleContainer.EndTime = DateTimeOffset.Now;
						moduleContainer.CompletedTask.Start();
						ConsoleRenderer.WriteSkippedModule(moduleContainer);
					}
					else
					{
						ConsoleRenderer.WriteModuleStarting(moduleContainer);
						moduleContainer.State = ModuleState.Running;
						moduleContainer.StartTime = DateTimeOffset.Now;
						await moduleContainer.Module.RunModule(ct);
						moduleContainer.HasCompleted = true;
						moduleContainer.CompletionType = CompletionType.Success;
						moduleContainer.State = ModuleState.Completed;
						moduleContainer.EndTime = DateTimeOffset.Now;
						moduleContainer.CompletedTask.Start();
						ConsoleRenderer.WriteModuleSuccess(moduleContainer);
					}
				}
				catch (Exception ex)
				{
					if (ex is TaskCanceledException or OperationCanceledException)
					{
						moduleContainer.CompletionType = CompletionType.Cancelled;
						ConsoleRenderer.WriteCancelledModule(moduleContainer);
					}
					else
					{
						moduleContainer.CompletionType = CompletionType.Failure;
						moduleContainer.ExceptionMessage = ex.Message;
						ConsoleRenderer.WriteModuleFailure(moduleContainer);
					}
					moduleContainer.HasCompleted = true;
					moduleContainer.State = ModuleState.Completed;
					moduleContainer.EndTime = DateTimeOffset.Now;
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
				moduleContainers.Where(s => s.State == ModuleState.Waiting).ToList().ForEach(c =>
				{
					c.HasCompleted = true;
					c.CompletionType = CompletionType.Cancelled;
					c.State = ModuleState.Completed;
					c.StartTime = DateTimeOffset.Now;
					c.EndTime = DateTimeOffset.Now;
					c.CompletedTask.Start();
				});
			}
			ConsoleRenderer.WriteFinalState(moduleContainers);
			AnsiConsole.WriteLine();
			moduleContainers.Where(s => s.ExceptionMessage != null).ToList().ForEach(s => AnsiConsole.WriteLine($"❌ {s.Module.GetType().Name} Failed: {s.ExceptionMessage}"));
			AnsiConsole.WriteLine($"OrchestratorService {(_isPipelineCancellationRequested ? "cancelled" : "completed")}");
			DeploymentTimeProvider.DeploymentEndTime = DateTimeOffset.Now;
		}
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
