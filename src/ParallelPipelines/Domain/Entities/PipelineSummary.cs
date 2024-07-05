using ParallelPipelines.Domain.Enums;

namespace ParallelPipelines.Domain.Entities;

public class PipelineSummary
{
	public CompletionType? OverallCompletionType { get; set; }
	public DateTimeOffset? DeploymentStartTime;
	public DateTimeOffset? DeploymentEndTime;
	public TimeSpan? DeploymentDuration => DeploymentEndTime - DeploymentStartTime;
	public List<StepContainer> StepContainers { get; set; } = [];
}
