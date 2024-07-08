using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Example.Deploy.Steps._1Setup;
using Example.Deploy.Steps._2ProvisionAzureResources;
using Example.Deploy.Steps._3BuildAndPublish;
using Example.Deploy.Steps._4Deploy;
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
	options.Cicd.UseDotnetArtifactOnRetry = true;
	options.Cicd.OutputSummaryToGithubStepSummary = true;
	options.Cicd.WriteCliCommandOutputsToSummary = true;
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
