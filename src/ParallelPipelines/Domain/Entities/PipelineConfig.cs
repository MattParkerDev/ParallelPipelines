namespace ParallelPipelines.Domain.Entities;

public class PipelineConfig
{
	public bool EnableGithubMarkdownTableSummary { get; set; } = true;
	public bool EnableGithubMarkdownGanttSummary { get; set; } = true;
	public bool WriteGithubActionSummaryToLocalFileLocally { get; set; } = false;
	public bool OpenGithubActionSummaryInVscodeLocallyAutomatically { get; set; } = false;
	public bool WriteCliCommandOutputsToSummaryFile { get; set; } = true;
}
