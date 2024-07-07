namespace ParallelPipelines.Domain.Entities;

public class PipelineConfig
{
	public CicdConfig Cicd { get; set; } = new();
	public LocalConfig Local { get; set; } = new();
	public List<string>? AllowedEnvironmentNames { get; set; }
}

public class CicdConfig
{
	public bool OutputSummaryToGithubStepSummary { get; set; } = false;
	public bool DisableGithubMarkdownTableSummary { get; set; } = false;
	public bool DisableGithubMarkdownGanttSummary { get; set; } = false;
	public bool WriteCliCommandOutputsToSummary { get; set; } = false;
}

public class LocalConfig
{
	public bool OutputSummaryToFile { get; set; } = false;
	public bool OpenSummaryFileInVscodeAutomatically { get; set; } = false;

	public bool DisableGithubMarkdownTableSummary { get; set; } = false;
	public bool DisableGithubMarkdownGanttSummary { get; set; } = false;
	public bool DisableWriteCliCommandOutputsToSummary { get; set; } = false;
}
