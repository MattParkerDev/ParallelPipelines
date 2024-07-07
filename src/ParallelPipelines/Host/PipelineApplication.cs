using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Actions.Octokit;
using GitHub;
using GitHub.Models;
using Microsoft.Extensions.Hosting;
using Octokit;
using Octokit.Internal;
using ParallelPipelines.Application;
using ParallelPipelines.Domain.Entities;
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
			var runId = Context.Current.RunId;
			var owner = Context.Current.Repo.Owner;
			var repo = Context.Current.Repo.Repo;
			var githubToken = _pipelineContext.Configuration["WorkflowGithubToken"];
			//var githubClient = new GitHubClient(new ProductHeaderValue("ParallelPipelines"), new InMemoryCredentialStore(new Credentials(githubToken, AuthenticationType.Bearer)));
			var githubClient = new GitHubClient(new ProductHeaderValue("ParallelPipelines"), new InMemoryCredentialStore(new Credentials("", AuthenticationType.Bearer)));
			owner = "MattParkerDev";
			repo = "ParallelPipelines";
			runId = 9828664485;
			var run = await githubClient.Actions.Workflows.Runs.Get(owner, repo, runId);
			var attemptNumber = run.RunAttempt + 1;
			PipelineSummaryDto? failedRunSummaryDto = null;
			if (attemptNumber > 1)
			{
				AnsiConsole.WriteLine($"ParallelPipelines is running on attempt number {attemptNumber}");
				var result = await githubClient.Actions.Artifacts.ListWorkflowArtifacts(owner, repo, runId);
				var artifacts = result.Artifacts;
				var failedRunArtifact = artifacts.FirstOrDefault(s => s.Name == "parallel-pipelines-artifact");
				if (failedRunArtifact is not null)
				{
					await using var stream = await githubClient.Actions.Artifacts.DownloadArtifact(owner, repo, failedRunArtifact.Id, "zip");
					// unzip stream and get json string
					var zipArchive = new ZipArchive(stream);
					var entry = zipArchive.Entries.FirstOrDefault();
					if (entry is not null)
					{
						await using var entryStream = entry.Open();
						failedRunSummaryDto = await JsonSerializer.DeserializeAsync<PipelineSummaryDto>(entryStream, cancellationToken: cancellationToken);
					}
				}
			}


			var pipelineSummary = await _orchestratorService.RunPipeline(failedRunSummaryDto, cancellationToken);
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
