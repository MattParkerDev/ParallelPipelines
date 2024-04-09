using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Host.Services;

public class ModuleContainerProvider(IEnumerable<ModuleContainer> moduleContainers)
{
	private readonly List<ModuleContainer> _moduleContainers = moduleContainers.ToList();

	public List<ModuleContainer> GetAllModuleContainers()
	{
		return _moduleContainers;
	}

	public async IAsyncEnumerable<ModuleContainer> GetModuleContainersOrderedForExecution()
	{
		var noDependencies = _moduleContainers.Where(m => m.Module.GetType().HasNoDependencies());
		foreach (var container in noDependencies)
		{
			yield return container;
		}
	}
}
