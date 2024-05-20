namespace ParallelPipelines.Domain.Entities;

public class PipelineConfig
{
	public CicdConfig Cicd { get; set; } = new();
	public LocalConfig Local { get; set; } = new();
}

public class CicdConfig
{
	public bool EnableGithubMarkdownTableSummary { get; set; } = true;
	public bool EnableGithubMarkdownGanttSummary { get; set; } = true;
	public bool WriteCliCommandOutputsToSummary { get; set; } = false;
}

public class LocalConfig
{
	public bool OutputSummaryToFile { get; set; } = false;
	public bool OpenSummaryFileInVscodeAutomatically { get; set; } = false;

	public bool EnableGithubMarkdownTableSummary { get; set; } = true;
	public bool EnableGithubMarkdownGanttSummary { get; set; } = true;
	public bool WriteCliCommandOutputsToSummary { get; set; } = true;
}
