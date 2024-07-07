using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;

namespace ParallelPipelines.Application;

public class StepContainerDto
{
	public string? StepName { get; set; }
	public CompletionType? CompletionType { get; set; }
	public DateTimeOffset? StartTime { get; set; }
	public DateTimeOffset? EndTime { get; set; }
}

public static class StepContainerMapper
{
	public static StepContainerDto ToDto(StepContainer stepContainer)
	{
		return new StepContainerDto
		{
			StepName = stepContainer.GetStepName(),
			CompletionType = stepContainer.CompletionType,
			StartTime = stepContainer.StartTime,
			EndTime = stepContainer.EndTime
		};
	}
}
