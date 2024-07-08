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

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, OrchestratorService orchestratorService, PostStepService postStepService, IPipelineContext pipelineContext, PreStepService preStepService)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly OrchestratorService _orchestratorService = orchestratorService;
	private readonly PostStepService _postStepService = postStepService;
	private readonly PreStepService _preStepService = preStepService;
	private readonly IPipelineContext _pipelineContext = pipelineContext;
	private readonly Stopwatch _timer = new Stopwatch();

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _orchestratorService.InitialiseAsync(cancellationToken);
		await _preStepService.RunPreSteps(cancellationToken);
		var failedRunSummaryDto = await _preStepService.GetFailedRunSummary(cancellationToken);
		_timer.Start();
		try
		{


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
