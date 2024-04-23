namespace ParallelPipelines.Domain.Entities;

public class PipelineConfig
{
	public bool EnableGithubMarkdownTableSummary { get; set; } = true;
	public bool EnableGithubMarkdownGanttSummary { get; set; } = true;
}
