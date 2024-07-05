using System.Runtime.CompilerServices;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Application.Attributes;

namespace ParallelPipelines.Host.Services;

public class StepContainerProvider(IEnumerable<StepContainer> stepContainers)
{
	private readonly List<StepContainer> _stepContainers = stepContainers.ToList();

	public List<StepContainer> GetAllStepContainers()
	{
		return _stepContainers;
	}

	public async IAsyncEnumerable<StepContainer> GetStepContainersOrderedForExecution(
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var noDependencies = _stepContainers.Where(m => m.Step.GetType().HasNoDependencies()).ToList();
		if (noDependencies.Count == 0)
		{
			throw new InvalidOperationException("No steps found with no dependencies");
		}

		foreach (var container in noDependencies)
		{
			yield return container;
		}

		var inProgress = noDependencies;
		var remaining = _stepContainers.Except(inProgress).ToList();

		while (remaining.Count != 0)
		{
			var result = await await Task.WhenAny(inProgress.Select(s => s.CompletedTask).ToList()).WaitAsync(cancellationToken); // required, as otherwise the loop will not be cancellable
			inProgress.Remove(result);
			foreach (var dependent in result.Dependents)
			{
				if (remaining.Contains(dependent) is false || dependent.Dependencies.Any(d => !d.HasCompleted))
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
