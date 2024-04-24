using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Host.Services.GithubActions;

public class GithubActionTableSummaryService
{
	public string GenerateTableSummary(PipelineSummary pipelineSummary)
	{
		var moduleStringList = pipelineSummary.ModuleContainers?.OrderBy(x => x.EndTime).ThenBy(s => s.StartTime).Select(
			moduleContainer =>
			{
				var (startTime, endTime, duration) = ConsoleRenderer.GetTimeStartedAndFinished(moduleContainer);
				var text = $"| {moduleContainer.GetModuleName()} | {ConsoleRenderer.GetStatusString(moduleContainer).ToDisplayString()} | {startTime} | {endTime} | {duration} |";
				return text;
			}
		).ToList() ?? [];

		var (globalStartTime, globalEndTime, globalDuration) = ConsoleRenderer.GetTimeStartedAndFinishedGlobal();
		var pipelineStatusString = ConsoleRenderer.GetStatusString(pipelineSummary?.OverallCompletionType).ToDisplayString();
		var overallSummaryString = $"| Total | **{pipelineStatusString}** | **{globalStartTime}** | **{globalEndTime}** | **{globalDuration}** |";
		var text = $"""
		            ### Run Summary
		            | Module | Status | Start | End | Duration |
		            | --- | --- | --- | --- | --- |
		            {string.Join("\n", moduleStringList)}
		            {overallSummaryString}
		            """;

		return text;
	}


}

file static class GithubMarkdownTableFormatter
{
	public static string ToDisplayString(this string status)
	{
		var text = status switch
		{
			"Success" => $$$"""${\textsf{\color{lightgreen}{{{status}}}}}$""",
			"Skipped" => $$$"""${\textsf{\color{orange}{{{status}}}}}$""",
			"Cancelled" => $$$"""${\textsf{\color{red}{{{status}}}}}$""",
			"Failure" => $$$"""${\textsf{\color{red}{{{status}}}}}$""",
			_ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
		};
		return text;
	}
}
