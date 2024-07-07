using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Example.Deploy.Steps._1Setup;
using ParallelPipelines.Domain.Enums;
using ParallelPipelines.Host;
using ParallelPipelines.Host.Services;
using ParallelPipelines.Unit.Tests.TestSteps;
using Spectre.Console;
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
		services.AddStep<InstallDotnetWasmToolsStep>();

		services.RemoveAll(typeof(IAnsiConsole));
		services.AddSingleton<IAnsiConsole>(provider => new MyTestConsole(_output));

		var serviceProvider = services.BuildServiceProvider();
		var orchestratorService = serviceProvider.GetRequiredService<OrchestratorService>();

		var pipelineSummary = await orchestratorService.RunPipeline(null, CancellationToken.None);
		pipelineSummary.Should().NotBeNull();
		pipelineSummary?.OverallCompletionType.Should().Be(CompletionType.Success);
	}

	[Fact]
	public async Task OrchestratorService_TestTimings()
	{
		var services = new ServiceCollection();
		services.AddParallelPipelines(new ConfigurationBuilder().Build(), options =>
		{
			options.Local.DisableGithubMarkdownGanttSummary = false;
			options.Local.DisableGithubMarkdownTableSummary = false;
		});
		services.AddStep<TestStep1>();
		services.AddStep<TestStep2>();
		services.AddStep<TestStep3>();
		services.AddStep<TestStep4>();

		services.RemoveAll(typeof(IAnsiConsole));
		services.AddSingleton<IAnsiConsole>(provider => new MyTestConsole(_output));

		var serviceProvider = services.BuildServiceProvider();
		var orchestratorService = serviceProvider.GetRequiredService<OrchestratorService>();
		var cancellationToken = new CancellationTokenSource().Token;
		await orchestratorService.InitialiseAsync(cancellationToken);
		var now = DateTimeOffset.Now;
		var pipelineSummary = await orchestratorService.RunPipeline(null, CancellationToken.None);
		var later = DateTimeOffset.Now;

		pipelineSummary.Should().NotBeNull();
		pipelineSummary?.OverallCompletionType.Should().Be(CompletionType.Success);

		pipelineSummary?.DeploymentStartTime.Should().BeAfter(now);
		pipelineSummary?.DeploymentStartTime.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(15));

		pipelineSummary?.DeploymentEndTime.Should().BeBefore(later);
		pipelineSummary?.DeploymentEndTime.Should().BeCloseTo(later, TimeSpan.FromMilliseconds(15));
		_output.WriteLine($"Deployment duration: {pipelineSummary?.DeploymentDuration}");

		pipelineSummary?.DeploymentDuration.Should().BeCloseTo(TimeSpan.FromMilliseconds(3000), TimeSpan.FromMilliseconds(50));

		var step1 = pipelineSummary?.StepContainers?.FirstOrDefault(x => x.Step.GetType() == typeof(TestStep1));
		step1.Should().NotBeNull();
		step1!.StartTime.Should().BeCloseTo(pipelineSummary!.DeploymentStartTime!.Value, TimeSpan.FromMilliseconds(10));

		step1!.EndTime.Should().BeAfter(step1.StartTime!.Value);
		step1!.EndTime.Should().BeCloseTo(step1.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(100));
		step1!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		var step2 = pipelineSummary?.StepContainers?.FirstOrDefault(x => x.Step.GetType() == typeof(TestStep2));
		step2.Should().NotBeNull();
		step2!.StartTime.Should().BeCloseTo(step1.EndTime!.Value, TimeSpan.FromMilliseconds(10));

		step2!.EndTime.Should().BeAfter(step2.StartTime!.Value);
		step2!.EndTime.Should().BeCloseTo(step2.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(50));
		step2!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		var step3 = pipelineSummary?.StepContainers?.FirstOrDefault(x => x.Step.GetType() == typeof(TestStep3));
		step3.Should().NotBeNull();
		step3!.StartTime.Should().BeCloseTo(step1.EndTime!.Value, TimeSpan.FromMilliseconds(10));
		step3!.StartTime.Should().BeCloseTo(step2.StartTime!.Value, TimeSpan.FromMilliseconds(10));

		step3!.EndTime.Should().BeAfter(step3.StartTime!.Value);
		step3!.EndTime.Should().BeCloseTo(step3.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(100));
		step3!.EndTime.Should().BeCloseTo(step2.EndTime!.Value, TimeSpan.FromMilliseconds(100));
		step3!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		var step4 = pipelineSummary?.StepContainers?.FirstOrDefault(x => x.Step.GetType() == typeof(TestStep4));

		step4.Should().NotBeNull();
		step4!.StartTime.Should().BeAfter(step2.EndTime!.Value);
		step4!.StartTime.Should().BeAfter(step3.EndTime!.Value);
		step4!.StartTime.Should().BeCloseTo(step2.EndTime!.Value, TimeSpan.FromMilliseconds(10));
		step4!.StartTime.Should().BeCloseTo(step3.EndTime!.Value, TimeSpan.FromMilliseconds(10));

		step4!.EndTime.Should().BeAfter(step4.StartTime!.Value);
		step4!.EndTime.Should().BeCloseTo(step4.StartTime!.Value.AddMilliseconds(1000), TimeSpan.FromMilliseconds(100));
		step4!.Duration.Should().BeCloseTo(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(100));

		pipelineSummary?.DeploymentEndTime.Should().BeCloseTo(step4.EndTime!.Value, TimeSpan.FromMilliseconds(10));
	}
}
