using Deploy.Experimental.Modules.BuildAndPublish;
using Deploy.Experimental.Modules.ProvisionAzureResources;
using Deploy.Modules.BuildAndPublish;
using Deploy.Modules.Deploy;
using Deploy.Modules.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ParallelPipelines.Host;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
	.AddJsonFile("appsettings.Development.json", true)
	.AddUserSecrets<Program>()
	.AddEnvironmentVariables();

builder.Services.AddParallelPipelines(builder.Configuration, options =>
{
	options.Cicd.WriteCliCommandOutputsToSummary = true;
	options.Local.OutputSummaryToFile = true;
});

builder.Services
	.AddModule<InstallDotnetWasmToolsModule>()
	.AddModule<InstallSwaCliModule>()
	.AddModule<RestoreAndBuildModule>()
	.AddModule<PublishWebApiModule>()
	.AddModule<PublishWebUiModule>()
	.AddModule<CreateResourceGroupModule>()
	.AddModule<DeployBicepModule>()
	.AddModule<DeployWebApiModule>()
	.AddModule<DeployWebUiModule>();

using var host = builder.Build();

await host.RunAsync();
