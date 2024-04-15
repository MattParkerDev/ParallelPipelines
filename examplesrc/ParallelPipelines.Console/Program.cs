using System.Text;
using Deploy.Experimental.Modules.BuildAndPublish;
using Deploy.Experimental.Modules.ProvisionAzureResources;
using Deploy.Modules.BuildAndPublish;
using Deploy.Modules.Deploy;
using Deploy.Modules.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ParallelPipelines.Host;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;
AnsiConsole.WriteLine("\x1b[36m📦 Bootstrapping...\x1b[0m");
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
	.AddJsonFile("appsettings.Development.json", true)
	.AddUserSecrets<Program>()
	.AddEnvironmentVariables();

builder.Services.AddModularPipelines(builder.Configuration);
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
