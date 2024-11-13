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

		var remaining = _stepContainers.Except(noDependencies).ToList();

		var tasks = Task.WhenEach(_stepContainers.Select(s => s.CompletedTask).ToList());
		await foreach (var task in tasks.WithCancellation(cancellationToken).ConfigureAwait(false))
		{
			var result = await task;
			foreach (var dependent in result.Dependents)
			{
				if (remaining.Contains(dependent) is false || dependent.Dependencies.Any(d => d.HasCompleted is false))
				{
					continue;
				}

				remaining.Remove(dependent);
				yield return dependent;
			}
		}
	}
}
