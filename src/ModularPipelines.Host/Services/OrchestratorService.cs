using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Host.Services;

public class OrchestratorService(IEnumerable<IModuleContainer> moduleContainers, IServiceProvider serviceProvider)
{
	private readonly IEnumerable<IModuleContainer> _moduleContainers = moduleContainers;
	private readonly IServiceProvider _serviceProvider = serviceProvider;

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
