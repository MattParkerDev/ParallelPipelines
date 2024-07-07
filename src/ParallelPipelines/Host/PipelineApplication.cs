using System.Diagnostics;
using Actions.Octokit;
using GitHub;
using GitHub.Models;
using Microsoft.Extensions.Hosting;
using Octokit;
using Octokit.Internal;
using ParallelPipelines.Host.Helpers;
using ParallelPipelines.Host.Services;
using Spectre.Console;
using GitHubClient = Octokit.GitHubClient;

namespace ParallelPipelines.Host;

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, OrchestratorService orchestratorService, PostStepService postStepService, IPipelineContext pipelineContext)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly OrchestratorService _orchestratorService = orchestratorService;
	private readonly PostStepService _postStepService = postStepService;
	private readonly IPipelineContext _pipelineContext = pipelineContext;
	private readonly Stopwatch _timer = new Stopwatch();

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _orchestratorService.InitialiseAsync(cancellationToken);
		_timer.Start();
		try
		{
			//var gitHubClient = new GitHubClient(new ProductHeaderValue("ParallelPipelines"));
			var workflowId = Context.Current.Workflow;
			var runId = Context.Current.RunId;
			var owner = Context.Current.Repo.Owner;
			var repo = Context.Current.Repo.Repo;
			var githubToken = _pipelineContext.Configuration["WorkflowGithubToken"];
			//var githubClient = new GitHubClient(new ProductHeaderValue("ParallelPipelines"), new InMemoryCredentialStore(new Credentials(githubToken, AuthenticationType.Bearer)));
			var githubClient = new GitHubClient(new ProductHeaderValue("ParallelPipelines"));
			owner = "MattParkerDev";
			repo = "ParallelPipelines";
			var runs = await githubClient.Actions.Workflows.Runs.ListByWorkflow(owner, repo, "example-prod-deploy.yml");

			var pipelineSummary = await _orchestratorService.RunPipeline(cancellationToken);
			await _postStepService.RunPostSteps(pipelineSummary, cancellationToken);
		}
		catch (Exception e)
		{
			if (e is OperationCanceledException or TaskCanceledException)
			{
				AnsiConsole.WriteLine($"ParallelPipelines failed with exception: {e.Message}");
				Environment.ExitCode = 1;
			}
			else
			{
				AnsiConsole.WriteLine($"ParallelPipelines failed with unhandled exception: {e.Message}");
				throw;
			}
		}
		_hostApplicationLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_timer.Stop();
		var timeString = _timer.Elapsed.ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s");
		AnsiConsole.WriteLine($"ParallelPipelines finished in {timeString}");
		AnsiConsole.WriteLine();
		return Task.CompletedTask;
	}
}
