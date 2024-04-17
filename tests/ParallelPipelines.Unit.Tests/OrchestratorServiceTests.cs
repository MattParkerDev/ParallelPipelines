using Deploy.Modules.Setup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host;
using ParallelPipelines.Host.Services;

namespace ParallelPipelines.Unit.Tests;

public class OrchestratorServiceTests
{
	[Fact]
	public async Task OrchestratorService_ReturnsSuccess()
	{
		var services = new ServiceCollection();
		services.AddParallelPipelines(new ConfigurationBuilder().Build());
		services.AddModule<InstallDotnetWasmToolsModule>();
		var serviceProvider = services.BuildServiceProvider();
		var orchestratorService = serviceProvider.GetRequiredService<OrchestratorService>();

		var pipelineSummary = await orchestratorService.RunPipeline(CancellationToken.None);
		pipelineSummary.Should().NotBeNull();
		pipelineSummary?.OverallCompletionType.Should().Be(CompletionType.Success);
	}
}
