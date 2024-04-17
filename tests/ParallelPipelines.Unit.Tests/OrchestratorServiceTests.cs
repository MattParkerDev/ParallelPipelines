using Deploy.Modules.Setup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host;
using ParallelPipelines.Host.Services;
using ParallelPipelines.Unit.Tests.TestModules;
using Xunit.Abstractions;


namespace ParallelPipelines.Unit.Tests;

public class OrchestratorServiceTests(ITestOutputHelper output)
{
	private readonly ITestOutputHelper _output = output;

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

	[Fact]
	public async Task OrchestratorService_TestTimings()
	{
		var services = new ServiceCollection();
		services.AddParallelPipelines(new ConfigurationBuilder().Build());
		services.AddModule<TestModule1>();
		services.AddModule<TestModule2>();
		services.AddModule<TestModule3>();
		services.AddModule<TestModule4>();

		var serviceProvider = services.BuildServiceProvider();
		var orchestratorService = serviceProvider.GetRequiredService<OrchestratorService>();

		var now = DateTimeOffset.Now;
		var pipelineSummary = await orchestratorService.RunPipeline(CancellationToken.None);
		var later = DateTimeOffset.Now;

		pipelineSummary.Should().NotBeNull();
		pipelineSummary?.OverallCompletionType.Should().Be(CompletionType.Success);

		pipelineSummary?.DeploymentStartTime.Should().BeAfter(now);
		pipelineSummary?.DeploymentStartTime.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(15));

		pipelineSummary?.DeploymentEndTime.Should().BeBefore(later);
		pipelineSummary?.DeploymentEndTime.Should().BeCloseTo(later, TimeSpan.FromMilliseconds(15));
		_output.WriteLine($"Deployment duration: {pipelineSummary?.DeploymentDuration}");
	}
}
