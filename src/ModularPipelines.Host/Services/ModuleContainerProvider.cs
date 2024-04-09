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
		var inProgress = noDependencies.ToList();
		var remaining = _moduleContainers.Except(inProgress).ToList();


		while (remaining.Count != 0)
		{
			var result = await Task.WhenAny(remaining.Select(s => s.CompletedSuccessfullyTask).ToList()).ConfigureAwait(false);
			var modulesThatModuleDependsOn = result.Result.Module.GetType().GetDependencies();
			var test = 0;
			// var modulesThatDependOnResult = remaining.Where(m => m.Module.GetType().DependsOn(result.Module.GetType()));
			// var next = remaining.FirstOrDefault(m => m.Module.GetType().DependenciesAreSatisfiedBy(_moduleContainers));
			// if (next == null)
			// {
			// 	throw new Exception("Circular dependency detected");
			// }
			// yield return next;
			// remaining.Remove(next);
		}
	}
}
