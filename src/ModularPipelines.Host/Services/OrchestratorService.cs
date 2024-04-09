using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Host.Services;

public class OrchestratorService(ModuleContainerProvider moduleContainerProvider)
{
	private readonly ModuleContainerProvider _moduleContainerProvider = moduleContainerProvider;

	public async Task RunPipeline(CancellationToken cancellationToken)
	{
		Console.WriteLine("🚀Executing OrchestratorService");
		var moduleContainers = _moduleContainerProvider.GetAllModuleContainers();
		LogFoundModules(moduleContainers);
		PopulateDependents(moduleContainers);

		var modulesToExecute = _moduleContainerProvider.GetModuleContainersOrderedForExecution(cancellationToken);

		await Parallel.ForEachAsync(modulesToExecute, cancellationToken, async (moduleContainer, ct) =>
		{
			try
			{
				await moduleContainer.Module.RunModule(ct);
				moduleContainer.HasCompletedSuccessfully = true;
				moduleContainer.CompletedSuccessfullyTask.Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running module {moduleContainer.Module.GetType().Name}: {ex.Message}");
				moduleContainer.HasCompletedSuccessfully = false;
			}
		}).ConfigureAwait(false);

		Console.WriteLine("OrchestratorService Complete");
	}

	private void LogFoundModules(List<ModuleContainer> moduleContainers)
	{
		moduleContainers.ForEach(c => Console.WriteLine($"⭐ Found {c.Module.GetType().Name}"));
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
}
