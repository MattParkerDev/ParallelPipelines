using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using ParallelPipelines.Host.Services;
using Spectre.Console;

namespace ParallelPipelines.Host;

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, OrchestratorService orchestratorService, PostStepService postStepService)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly OrchestratorService _orchestratorService = orchestratorService;
	private readonly PostStepService _postStepService = postStepService;
	private readonly Stopwatch _timer = new Stopwatch();

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _orchestratorService.InitialiseAsync(cancellationToken);
		_timer.Start();
		try
		{
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
