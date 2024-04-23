using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Services.GithubActions;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class PostStepService(GithubActionTableSummaryService githubActionTableSummaryService)
{
	private readonly GithubActionTableSummaryService _githubActionTableSummaryService = githubActionTableSummaryService;

	public async Task RunPostSteps(PipelineSummary pipelineSummary, CancellationToken cancellationToken)
	{
		var tableSummary = _githubActionTableSummaryService.GenerateTableSummary(pipelineSummary);
		AnsiConsole.WriteLine(tableSummary);
	}
}
