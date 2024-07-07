using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;

namespace ParallelPipelines.Application;

public class PipelineSummaryDto
{
	public CompletionType? OverallCompletionType { get; set; }
	public DateTimeOffset? DeploymentStartTime;
	public DateTimeOffset? DeploymentEndTime;
	public List<StepContainerDto> StepContainers { get; set; } = [];
}

public static class PipelineSummaryMapper
{
	public static PipelineSummaryDto ToDto(PipelineSummary pipelineSummary)
	{
		return new PipelineSummaryDto
		{
			OverallCompletionType = pipelineSummary.OverallCompletionType,
			DeploymentStartTime = pipelineSummary.DeploymentStartTime,
			DeploymentEndTime = pipelineSummary.DeploymentEndTime,
			StepContainers = pipelineSummary.StepContainers.Select(StepContainerMapper.ToDto).ToList()
		};
	}
}
