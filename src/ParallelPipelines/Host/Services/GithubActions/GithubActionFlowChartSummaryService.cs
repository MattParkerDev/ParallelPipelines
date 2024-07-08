using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host.InternalHelpers;

namespace ParallelPipelines.Host.Services.GithubActions;

public class GithubActionFlowChartSummaryService
{
	public string GenerateFlowchartSummary(PipelineSummary pipelineSummary)
	{
		var stepStringList = pipelineSummary.StepContainers?.OrderBy(x => x.EndTime).ThenBy(s => s.StartTime).Select(
			x =>
			{
				var (startTime, endTime) = x.GetTimeStartedAndFinished();
				return x.GetStepName();
			}).ToList() ?? [];

		Dictionary<string, List<string>> stepListAndDependents = pipelineSummary.StepContainers?.OrderBy(x => x.EndTime).ThenBy(s => s.StartTime).Select(
			x =>
			{
				var (startTime, endTime) = x.GetTimeStartedAndFinished();
				return (x.GetStepName(), x.Dependents.Select(d => d.GetStepName()).ToList());
			}).ToDictionary(x => x.Item1, x => x.Item2) ?? new();

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
		            ---

		            flowchart LR


		            {string.Join("\n", stepStringList)}
		            {string.Join("\n", stepListAndDependents.Select(x => x.Value.Select(d => $"{x.Key} --> {d}").ToList()).SelectMany(x => x))}



		            ```
		            """;

		return text;
	}
}

file static class GithubMarkdownGanttFormatter
{
	public static (string? startTime, string? endTime) GetTimeStartedAndFinished(this StepContainer step)
	{
		var startTime = step.StartTime - DeploymentTimeProvider.DeploymentStartTime;
		var endTime = step.EndTime - DeploymentTimeProvider.DeploymentStartTime;

		var startTimeOnly = TimeOnly.FromTimeSpan(startTime!.Value);
		var endTimeOnly = TimeOnly.FromTimeSpan(endTime!.Value);

		var startTimeString = startTimeOnly.ToString("mm:ss:fff");
		var endTimeString = endTimeOnly.ToString("mm:ss:fff");
		return (startTimeString, endTimeString);
	}
}
