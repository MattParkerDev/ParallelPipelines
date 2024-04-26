using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;

namespace ParallelPipelines.Host.Services.GithubActions;

public class GithubActionGanttSummaryService
{
	public string GenerateMermaidSummary(PipelineSummary pipelineSummary)
	{
		var moduleStringList = pipelineSummary.ModuleContainers?.OrderBy(x => x.EndTime).ThenBy(s => s.StartTime).Select(
			x =>
			{
				var (startTime, endTime) = x.GetTimeStartedAndFinished();
				return $"{x.GetModuleName()} :{AddCritIfFailed(x)} {startTime}, {endTime}";
			}).ToList() ?? [];

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
		            	dateFormat  mm:ss:SSS
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
}

file static class GithubMarkdownGanttFormatter
{
	public static (string? startTime, string? endTime) GetTimeStartedAndFinished(this ModuleContainer module)
	{
		var startTime = module.StartTime - DeploymentTimeProvider.DeploymentStartTime;
		var endTime = module.EndTime - DeploymentTimeProvider.DeploymentStartTime;

		var startTimeOnly = TimeOnly.FromTimeSpan(startTime!.Value);
		var endTimeOnly = TimeOnly.FromTimeSpan(endTime!.Value);

		var startTimeString = startTimeOnly.ToString("mm:ss:fff");
		var endTimeString = endTimeOnly.ToString("mm:ss:fff");
		return (startTimeString, endTimeString);
	}
}
