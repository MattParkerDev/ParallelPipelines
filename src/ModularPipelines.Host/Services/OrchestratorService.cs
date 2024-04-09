using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Host.Services;

public class OrchestratorService(IEnumerable<ModuleContainer> moduleContainers)
{
	private readonly IEnumerable<ModuleContainer> _moduleContainers = moduleContainers;

	public async Task RunPipeline()
	{
		Console.WriteLine("🚀Executing OrchestratorService");
		var moduleContainers = _moduleContainers.ToList();
		foreach (var container in moduleContainers)
		{
			Console.WriteLine($"⭐ Found {container.Module.GetType().Name}");
		}
		// find modules with no dependencies
		var noDependencies = moduleContainers.Where(m => m.Module.GetType().HasNoDependencies());

		await Parallel.ForEachAsync(noDependencies, async (moduleContainer, cancellationToken) =>
		{
			await moduleContainer.Module.RunModule();
		});

		Console.WriteLine("OrchestratorService Complete");
	}
}
