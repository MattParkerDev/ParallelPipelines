using System.IO.Compression;
using System.Text.Json;
using Actions.Octokit;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using ParallelPipelines.Application;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;
using ParallelPipelines.Host.InternalHelpers;
using ParallelPipelines.Host.Services.GithubActions;
using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class PreStepService(IOptions<PipelineConfig> pipelineConfig, IAnsiConsole console, IPipelineContext pipelineContext)
{
	private readonly IAnsiConsole _console = console;
	private readonly PipelineConfig _pipelineConfig = pipelineConfig.Value;
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task RunPreSteps(CancellationToken cancellationToken)
	{
		if (DeploymentConstants.IsGithubActions)
		{
			return;
		}
		if (_pipelineConfig.Cicd.UseDotnetArtifactOnRetry)
		{

		}
		await Task.Delay(1);
	}

	public async Task<PipelineSummaryDto?> GetFailedRunSummary(CancellationToken cancellationToken)
	{
		//var gitHubClient = new GitHubClient(new ProductHeaderValue("ParallelPipelines"));
		return null;
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

		return failedRunSummaryDto;
	}
}
