using Deploy.Modules.Setup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
		services.AddLogging(builder => builder.AddXUnit(_output));
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

		pipelineSummary?.DeploymentDuration.Should().BeCloseTo(TimeSpan.FromMilliseconds(3000), TimeSpan.FromMilliseconds(50));

		var module1 = pipelineSummary?.ModuleContainers?.FirstOrDefault(x => x.Module.GetType() == typeof(TestModule1));
		module1.Should().NotBeNull();
		module1!.StartTime.Should().BeCloseTo(pipelineSummary!.DeploymentStartTime!.Value, TimeSpan.FromMilliseconds(10));

		module1!.EndTime.Should().BeAfter(module1.StartTime!.Value);
		module1!.EndTime.Should().BeCloseTo(module1.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(100));
		module1!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		var module2 = pipelineSummary?.ModuleContainers?.FirstOrDefault(x => x.Module.GetType() == typeof(TestModule2));
		module2.Should().NotBeNull();
		module2!.StartTime.Should().BeCloseTo(module1.EndTime!.Value, TimeSpan.FromMilliseconds(10));

		module2!.EndTime.Should().BeAfter(module2.StartTime!.Value);
		module2!.EndTime.Should().BeCloseTo(module2.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(50));
		module2!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		var module3 = pipelineSummary?.ModuleContainers?.FirstOrDefault(x => x.Module.GetType() == typeof(TestModule3));
		module3.Should().NotBeNull();
		module3!.StartTime.Should().BeCloseTo(module1.EndTime!.Value, TimeSpan.FromMilliseconds(10));
		module3!.StartTime.Should().BeCloseTo(module2.StartTime!.Value, TimeSpan.FromMilliseconds(10));

		module3!.EndTime.Should().BeAfter(module3.StartTime!.Value);
		module3!.EndTime.Should().BeCloseTo(module3.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(100));
		module3!.EndTime.Should().BeCloseTo(module2.EndTime!.Value, TimeSpan.FromMilliseconds(100));
		module3!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		var module4 = pipelineSummary?.ModuleContainers?.FirstOrDefault(x => x.Module.GetType() == typeof(TestModule4));

		module4.Should().NotBeNull();
		module4!.StartTime.Should().BeCloseTo(module2.EndTime!.Value, TimeSpan.FromMilliseconds(10));
		module4!.StartTime.Should().BeCloseTo(module3.EndTime!.Value, TimeSpan.FromMilliseconds(10));

		module4!.EndTime.Should().BeAfter(module4.StartTime!.Value);
		module4!.EndTime.Should().BeCloseTo(module4.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(100));
		module4!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		pipelineSummary?.DeploymentEndTime.Should().BeCloseTo(module4.EndTime!.Value, TimeSpan.FromMilliseconds(10));
	}
}
