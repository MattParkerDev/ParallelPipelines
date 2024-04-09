using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Host.Services;

public class OrchestratorService(IEnumerable<IModule> modules)
{
	private readonly IEnumerable<IModule> _modules = modules;

	public async Task ExecuteAsync()
	{
		Console.WriteLine("🚀Executing OrchestratorService");
		var modules = _modules.ToList();
		foreach (var module in modules)
		{
			Console.WriteLine($"⭐ Found {module.GetType().Name}");
		}
		// find modules with no dependencies
		var noDependencies = modules.Where(m => m.GetType().HasNoDependencies());

		await Parallel.ForEachAsync(noDependencies, async (module, cancellationToken) =>
		{
			await module.RunModule();
		});

		Console.WriteLine("OrchestratorService Complete");
	}
}
