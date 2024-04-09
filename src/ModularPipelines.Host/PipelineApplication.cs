using Microsoft.Extensions.Hosting;
using ModularPipelines.Host.Services;

namespace ModularPipelines.Host;

public class PipelineApplication(IHostApplicationLifetime hostApplicationLifetime, ExampleAnsiProgressService progressService, OrchestratorService orchestratorService)
	: IHostedService
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
	private readonly ExampleAnsiProgressService _progressService = progressService;
	private readonly OrchestratorService _orchestratorService = orchestratorService;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("Starting PipelineApplication Hosted Service");

		await _orchestratorService.RunPipeline(cancellationToken);

		_hostApplicationLifetime.StopApplication();
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("PipelineApplication Hosted Service is stopping");
	}
}
