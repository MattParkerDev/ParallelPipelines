using System.Runtime.CompilerServices;
using Parker.ModularPipelines.Application.Attributes;
using Parker.ModularPipelines.Domain.Entities;

namespace Parker.ModularPipelines.Host.Services;

public class ModuleContainerProvider(IEnumerable<ModuleContainer> moduleContainers)
{
	private readonly List<ModuleContainer> _moduleContainers = moduleContainers.ToList();

	public List<ModuleContainer> GetAllModuleContainers()
	{
		return _moduleContainers;
	}

	public async IAsyncEnumerable<ModuleContainer> GetModuleContainersOrderedForExecution(
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var noDependencies = _moduleContainers.Where(m => m.Module.GetType().HasNoDependencies()).ToList();
		if (noDependencies.Count == 0)
		{
			throw new InvalidOperationException("No modules found with no dependencies");
		}

		foreach (var container in noDependencies)
		{
			yield return container;
		}

		var inProgress = noDependencies;
		var remaining = _moduleContainers.Except(inProgress).ToList();

		while (remaining.Count != 0)
		{
			var result = await await Task.WhenAny(inProgress.Select(s => s.CompletedTask).ToList()).WaitAsync(cancellationToken);
			inProgress.Remove(result);
			foreach (var dependent in result.Dependents)
			{
				if (dependent.Dependencies.Any(d => !d.HasCompleted))
				{
					continue;
				}

				yield return dependent;
				inProgress.Add(dependent);
				remaining.Remove(dependent);
			}
		}
	}
}
