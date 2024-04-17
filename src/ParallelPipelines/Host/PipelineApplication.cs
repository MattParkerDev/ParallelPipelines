using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using ParallelPipelines.Host.Services;
using Spectre.Console;

namespace ParallelPipelines.Host;

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, OrchestratorService orchestratorService)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly OrchestratorService _orchestratorService = orchestratorService;
	private readonly Stopwatch _timer = new Stopwatch();

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _orchestratorService.InitialiseAsync();
		_timer.Start();
		try
		{
			_ = await _orchestratorService.RunPipeline(cancellationToken);
		}
		catch (Exception)
		{
			AnsiConsole.WriteLine("ParallelPipelines failed");
			throw;
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
