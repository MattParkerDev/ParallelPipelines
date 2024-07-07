using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ParallelPipelines.Console.Steps._1Setup;
using ParallelPipelines.Console.Steps._2ProvisionAzureResources;
using ParallelPipelines.Console.Steps._3BuildAndPublish;
using ParallelPipelines.Console.Steps._4Deploy;
using ParallelPipelines.Host;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
#if DEBUG
	.AddJsonFile("appsettings.Development.json", true)
#endif
	.AddUserSecrets<Program>()
	.AddEnvironmentVariables();

builder.Services.AddParallelPipelines(builder.Configuration, options =>
{
	options.Local.OutputSummaryToFile = true;
	options.AllowedEnvironmentNames = ["dev", "prod"];
});

builder.Services
	.AddStep<InstallDotnetWasmToolsStep>()
	.AddStep<InstallSwaCliStep>()
	.AddStep<RestoreAndBuildStep>()
	.AddStep<PublishWebApiStep>()
	.AddStep<PublishWebUiStep>()
	.AddStep<CreateResourceGroupStep>()
	.AddStep<DeployBicepStep>()
	.AddStep<DeployWebApiStep>()
	.AddStep<DeployWebUiStep>();

using var host = builder.Build();

await host.RunAsync();
