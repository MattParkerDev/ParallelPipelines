using System.Diagnostics;
using Microsoft.Extensions.Options;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;
using ParallelPipelines.Host.InternalHelpers;
using ParallelPipelines.Host.Services.GithubActions;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class PostStepService(GithubActionTableSummaryService githubActionTableSummaryService, IOptions<PipelineConfig> pipelineConfig, GithubActionGanttSummaryService githubActionGanttSummaryService)
{
	private readonly GithubActionTableSummaryService _githubActionTableSummaryService = githubActionTableSummaryService;
	private readonly GithubActionGanttSummaryService _githubActionGanttSummaryService = githubActionGanttSummaryService;
	private readonly PipelineConfig _pipelineConfig = pipelineConfig.Value;

	public async Task RunPostSteps(PipelineSummary pipelineSummary, CancellationToken cancellationToken)
	{
		await WriteGithubSummary(pipelineSummary, cancellationToken);
	}
	private async Task WriteGithubSummary(PipelineSummary pipelineSummary, CancellationToken cancellationToken)
	{
		var text = string.Empty;
		if (_pipelineConfig.EnableGithubMarkdownTableSummary)
		{
			var tableSummary = _githubActionTableSummaryService.GenerateTableSummary(pipelineSummary);
			text += tableSummary;
		}
		if (_pipelineConfig.EnableGithubMarkdownGanttSummary)
		{
			var ganttSummary = _githubActionGanttSummaryService.GenerateMermaidSummary(pipelineSummary);
			text += "\n\n" + ganttSummary;
		}
		if (DeploymentConstants.IsGithubActions && string.IsNullOrWhiteSpace(text) is false)
		{
			var githubStepSummaryPath = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY")!;
			var githubStepSummary = await PipelineFileHelper.GetFile(githubStepSummaryPath);
			await File.WriteAllTextAsync(githubStepSummary.FullName, text, cancellationToken);
		}

		if (_pipelineConfig.OpenGithubActionSummaryInVscodeLocally && DeploymentConstants.IsGithubActions is false)
		{
			var githubStepSummaryLocal = await PipelineFileHelper.GitRootDirectory.CreateFileIfMissingAndGetFile("./artifacts/github-step-summary-local.md");
			await File.WriteAllTextAsync(githubStepSummaryLocal.FullName, "(Ctrl+Shift+V to open in pretty preview window)\n" + text, cancellationToken);

			//throw new ApplicationException("OpenGithubActionSummaryInVscodeLocally is not implemented yet.");
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "code",
				Arguments = githubStepSummaryLocal.FullName,
				UseShellExecute = true
			};
			Process.Start(processStartInfo);
		}
	}
}
