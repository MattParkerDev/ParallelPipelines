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

		try
		{
			await _orchestratorService.RunPipeline(cancellationToken);
		}
		catch (TaskCanceledException)
		{
			Console.WriteLine($"PipelineApplication was cancelled");
		}

		_hostApplicationLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("PipelineApplication Hosted Service is stopping");
		return Task.CompletedTask;
	}
}
