using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Host.Services;

public class OrchestratorService(ModuleContainerProvider moduleContainerProvider)
{
	private readonly ModuleContainerProvider _moduleContainerProvider = moduleContainerProvider;

	public async Task RunPipeline()
	{
		Console.WriteLine("🚀Executing OrchestratorService");
		var moduleContainers = _moduleContainerProvider.GetAllModuleContainers();
		moduleContainers.ForEach(c => Console.WriteLine($"⭐ Found {c.Module.GetType().Name}"));

		var tasks = new List<Task>();
		var modulesToExecute = _moduleContainerProvider.GetModuleContainersOrderedForExecution();
		await Parallel.ForEachAsync(modulesToExecute, async (moduleContainer, cancellationToken) =>
		{
			try
			{
				await moduleContainer.Module.RunModule();
				moduleContainer.HasCompletedSuccessfully = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running module {moduleContainer.Module.GetType().Name}: {ex.Message}");
				moduleContainer.HasCompletedSuccessfully = false;
			}
		});

		Console.WriteLine("OrchestratorService Complete");
	}
}
