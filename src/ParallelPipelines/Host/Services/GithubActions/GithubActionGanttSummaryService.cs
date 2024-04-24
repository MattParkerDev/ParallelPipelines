using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;

namespace ParallelPipelines.Host.Services.GithubActions;

public class GithubActionGanttSummaryService
{
	public string GenerateMermaidSummary(PipelineSummary pipelineSummary)
	{
		var moduleStringList = pipelineSummary.ModuleContainers?.OrderBy(x => x.EndTime).ThenBy(s => s.StartTime).Select(
			x =>
				$"{x.GetModuleName()} :{AddCritIfFailed(x)} {GetMinutesSeconds(x.StartTime!.Value)}, {GetMinutesSeconds(x.EndTime!.Value)}"
		).ToList() ?? [];

		var text = $"""
		            ```mermaid
		            ---
		            config:
		              theme: base
		              themeVariables:
		                primaryColor: "#007d15"
		                primaryTextColor: "#fff"
		                primaryBorderColor: "#02ad1e"
		                lineColor: "#F8B229"
		                secondaryColor: "#006100"
		                tertiaryColor: "#fff"
		                darkmode: "true"
		                titleColor: "#fff"
		              gantt:
		                leftPadding: 40
		                rightPadding: 120
		            ---

		            gantt
		            	dateFormat  mm:ss
		            	title       Run Summary
		            	axisFormat %M:%S

		            {string.Join("\n", moduleStringList)}
		            ```
		            """;

		return text;
	}

	private static string AddCritIfFailed(ModuleContainer moduleContainer)
	{
		return moduleContainer.CompletionType is CompletionType.Failure or CompletionType.Cancelled ? "crit," : string.Empty;
	}

	private static string GetMinutesSeconds(DateTimeOffset dateTimeOffset)
	{
		if (dateTimeOffset == DateTimeOffset.MinValue)
		{
			return string.Empty;
		}

		return dateTimeOffset.ToTimeOnly().ToString("mm:ss");
	}
}

file static class GithubMarkdownGanttFormatter
{
	public static TimeOnly ToTimeOnly(this DateTimeOffset dateTime)
	{
		return new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second);
	}
}
