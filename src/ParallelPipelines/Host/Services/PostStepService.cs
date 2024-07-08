using System.Diagnostics;
using Microsoft.Extensions.Options;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;
using ParallelPipelines.Host.InternalHelpers;
using ParallelPipelines.Host.Services.GithubActions;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class PostStepService(GithubActionTableSummaryService githubActionTableSummaryService, IOptions<PipelineConfig> pipelineConfig, GithubActionGanttSummaryService githubActionGanttSummaryService, IAnsiConsole console, GithubActionFlowChartSummaryService githubActionFlowChartSummaryService)
{
	private readonly GithubActionTableSummaryService _githubActionTableSummaryService = githubActionTableSummaryService;
	private readonly GithubActionGanttSummaryService _githubActionGanttSummaryService = githubActionGanttSummaryService;
	private readonly GithubActionFlowChartSummaryService _githubActionFlowChartSummaryService = githubActionFlowChartSummaryService;
	private readonly IAnsiConsole _console = console;
	private readonly PipelineConfig _pipelineConfig = pipelineConfig.Value;

	public async Task RunPostSteps(PipelineSummary pipelineSummary, CancellationToken cancellationToken)
	{
		var text = GetGithubSummary(pipelineSummary, cancellationToken);
		await WriteGithubSummary(text, cancellationToken);
	}

	private string GetGithubSummary(PipelineSummary pipelineSummary, CancellationToken cancellationToken)
	{
		var text = string.Empty;

		if (DeploymentConstants.IsGithubActions)
		{
			if (_pipelineConfig.Cicd.OutputSummaryToGithubStepSummary is false)
			{
				return text;
			}
			if (!_pipelineConfig.Cicd.DisableGithubMarkdownTableSummary)
			{
				var tableSummary = _githubActionTableSummaryService.GenerateTableSummary(pipelineSummary);
				text += tableSummary;
			}
			if (!_pipelineConfig.Cicd.DisableGithubMarkdownGanttSummary)
			{
				var ganttSummary = _githubActionGanttSummaryService.GenerateMermaidSummary(pipelineSummary);
				text += "\n\n" + ganttSummary;
			}
			if (!_pipelineConfig.Cicd.DisableGithubMarkdownFlowchartSummary)
			{
				var flowchartSummary = _githubActionFlowChartSummaryService.GenerateFlowchartSummary(pipelineSummary);
				text += "\n\n" + flowchartSummary;
			}
			if (_pipelineConfig.Cicd.WriteCliCommandOutputsToSummary)
			{
				var cliCommandSummary = GetCliCommandOutput(pipelineSummary);
				text += cliCommandSummary;
			}
		}
		else
		{
			if (_pipelineConfig.Local.OutputSummaryToFile is false)
			{
				return text;
			}
			if (!_pipelineConfig.Local.DisableGithubMarkdownTableSummary)
			{
				var tableSummary = _githubActionTableSummaryService.GenerateTableSummary(pipelineSummary);
				text += tableSummary;
			}
			if (!_pipelineConfig.Local.DisableGithubMarkdownGanttSummary)
			{
				var ganttSummary = _githubActionGanttSummaryService.GenerateMermaidSummary(pipelineSummary);
				text += "\n\n" + ganttSummary;
			}
			if (!_pipelineConfig.Local.DisableGithubMarkdownFlowchartSummary)
			{
				var flowchartSummary = _githubActionFlowChartSummaryService.GenerateFlowchartSummary(pipelineSummary);
				text += "\n\n" + flowchartSummary;
			}
			if (!_pipelineConfig.Local.DisableWriteCliCommandOutputsToSummary)
			{
				var cliCommandSummary = GetCliCommandOutput(pipelineSummary);
				text += cliCommandSummary;
			}
		}

		return text;
	}

	private static string GetCliCommandOutput(PipelineSummary pipelineSummary)
	{
		var text = "\n### CLI Command Outputs\n";
		foreach (var stepContainer in pipelineSummary.StepContainers.OrderBy(x => x.EndTime).ThenBy(s => s.StartTime))
		{
			var standardOutput = string.Join("\n", stepContainer.CliCommandResults?.Select(x => x?.StandardOutput) ?? Array.Empty<string>()).Trim();
			var errorOutput = string.Join("\n", stepContainer.CliCommandResults?.Select(x => x?.StandardError) ?? Array.Empty<string?>()).Trim();
			text += $"""
			         <details>
			         <summary>{stepContainer.GetStepName()}</summary>

			         ##### Error Output
			         ```console
			         {errorOutput}
			         ```

			         ##### Standard Output
			         ```console
			         {standardOutput}
			         ```

			         </details>
			         """;
		}

		return text;
	}

	private async Task WriteGithubSummary(string text, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		if (DeploymentConstants.IsGithubActions)
		{
			if (_pipelineConfig.Cicd.OutputSummaryToGithubStepSummary)
			{
				var githubStepSummaryPath = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY")!;
				var githubStepSummary = await PipelineFileHelper.GetFile(githubStepSummaryPath);
				await File.WriteAllTextAsync(githubStepSummary.FullName, text, cancellationToken);
			}
		}
		else if (_pipelineConfig.Local.OutputSummaryToFile)
		{
			var githubStepSummaryLocal = await PipelineFileHelper.GitRootDirectory.CreateFileIfMissingAndGetFile("./artifacts/github-step-summary-local.md");

			await File.WriteAllTextAsync(githubStepSummaryLocal.FullName, "(Ctrl+Shift+V to open in pretty preview window)\n" + text, cancellationToken);

			if (_pipelineConfig.Local.OpenSummaryFileInVscodeAutomatically)
			{
				var processStartInfo = new ProcessStartInfo
				{
					FileName = "code",
					Arguments = githubStepSummaryLocal.FullName,
					UseShellExecute = true
				};
				Process.Start(processStartInfo);
			}

			_console.WriteLine("\nWrote Github Action Summary to: file:///" + githubStepSummaryLocal.GetFullNameUnix() + "\n");
		}
	}
}
