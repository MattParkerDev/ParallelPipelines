using Application.Attributes;
using Domain.Entities;
using Domain.Enums;
using Spectre.Console;

namespace ModularPipelines.Host.Services;

public class OrchestratorService(ModuleContainerProvider moduleContainerProvider)
{
	private readonly ModuleContainerProvider _moduleContainerProvider = moduleContainerProvider;
	private readonly bool _exitPipelineOnSingleFailure = true;
	private bool _isPipelineCancellationRequested = false;

	public async Task RunPipeline(CancellationToken cancellationToken)
	{
		AnsiConsole.WriteLine("🚀Executing OrchestratorService");
		var moduleContainers = _moduleContainerProvider.GetAllModuleContainers();
		LogFoundModules(moduleContainers);
		PopulateDependents(moduleContainers);
		PopulateDependencies(moduleContainers);

		var modulesToExecute = _moduleContainerProvider.GetModuleContainersOrderedForExecution(cancellationToken);

		using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		await Parallel.ForEachAsync(modulesToExecute, linkedCts.Token, async (moduleContainer, ct) =>
		{
			try
			{
				if (_isPipelineCancellationRequested)
				{
					moduleContainer.HasCompleted = true;
					moduleContainer.CompletionType = CompletionType.Cancelled;
					moduleContainer.CompletedTask.Start();
					AnsiConsole.WriteLine($"{moduleContainer.Module.GetType().Name} cancelled due to previous failure");
				}
				else if (moduleContainer.Module.ShouldSkip())
				{
					moduleContainer.HasCompleted = true;
					moduleContainer.CompletionType = CompletionType.Skipped;
					moduleContainer.CompletedTask.Start();
					AnsiConsole.WriteLine($"{moduleContainer.Module.GetType().Name} skipped");
				}
				else
				{
					AnsiConsole.WriteLine($"⚡ {moduleContainer.Module.GetType().Name} Starting");
					await moduleContainer.Module.RunModule(ct);
					moduleContainer.HasCompleted = true;
					moduleContainer.CompletionType = CompletionType.Success;
					moduleContainer.CompletedTask.Start();
					AnsiConsole.WriteLine($"✅ {moduleContainer.Module.GetType().Name} Finished Successfully");
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.WriteLine($"❌ {moduleContainer.Module.GetType().Name} Failed: {ex.Message}");
				moduleContainer.HasCompleted = true;
				moduleContainer.CompletionType = CompletionType.Failure;
				if (_exitPipelineOnSingleFailure)
				{
					_isPipelineCancellationRequested = true;
					await linkedCts.CancelAsync();
				}

				moduleContainer.CompletedTask.Start();
			}
		});

		AnsiConsole.WriteLine("OrchestratorService Complete");
		moduleContainers.ForEach(c => AnsiConsole.WriteLine($"🏁 {c.Module.GetType().Name} {c.CompletionType}"));
	}

	private void LogFoundModules(List<ModuleContainer> moduleContainers)
	{
		moduleContainers.ForEach(c => AnsiConsole.WriteLine($"⭐ Found {c.Module.GetType().Name}"));
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
