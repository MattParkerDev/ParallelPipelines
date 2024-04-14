﻿using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Parker.ModularPipelines.Host.Helpers;
using Parker.ModularPipelines.Host.InternalHelpers;
using Parker.ModularPipelines.Host.Services;
using Spectre.Console;

namespace Parker.ModularPipelines.Host;

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, OrchestratorService orchestratorService)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly OrchestratorService _orchestratorService = orchestratorService;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await PipelineFileHelper.PopulateGitRootDirectory();
		DeploymentConstants.IsGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
		AnsiConsole.WriteLine("Starting PipelineApplication Hosted Service");
		var timer = Stopwatch.StartNew();
		try
		{
			await _orchestratorService.RunPipeline(cancellationToken);
		}
		catch (TaskCanceledException)
		{
			AnsiConsole.WriteLine("PipelineApplication was cancelled");
		}
		timer.Stop();
		var timeString = timer.Elapsed.ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s");
		AnsiConsole.WriteLine($"PipelineApplication Hosted Service finished in {timeString}");
		_hostApplicationLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		AnsiConsole.WriteLine("PipelineApplication Hosted Service is stopping");
		return Task.CompletedTask;
	}
}
