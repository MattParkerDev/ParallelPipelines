using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Host.Services;

namespace ModularPipelines.Host;

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, OrchestratorService orchestratorService)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly OrchestratorService _orchestratorService = orchestratorService;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("Starting PipelineApplication Hosted Service");
		var timer = Stopwatch.StartNew();
		try
		{
			await _orchestratorService.RunPipeline(cancellationToken);
		}
		catch (TaskCanceledException)
		{
			Console.WriteLine("PipelineApplication was cancelled");
		}
		timer.Stop();
		var timeString = timer.Elapsed.ToString(@"hh\h\:mm\m\:ss\s\:ff\m\s");
		Console.WriteLine($"PipelineApplication Hosted Service finished in {timeString}");
		_hostApplicationLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("PipelineApplication Hosted Service is stopping");
		return Task.CompletedTask;
	}
}
