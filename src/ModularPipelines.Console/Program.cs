using System.Text;
using Deploy.Modules.Setup;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Console.Modules;
using ModularPipelines.Host;

Console.OutputEncoding = Encoding.UTF8;
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModularPipelines(builder.Configuration);
builder.Services.AddModule<PowershellModule>();
builder.Services.AddModule<SecondModule>();
builder.Services.AddModule<InstallSwaCliModule>();
builder.Services.AddModule<InstallDotnetWasmToolsModule>();

using var host = builder.Build();

await host.RunAsync();
