using Microsoft.Extensions.Hosting;
using ModularPipelines.Console.Modules;
using ModularPipelines.Host;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModularPipelines(builder.Configuration);
builder.Services.AddModule<PowershellModule>();
builder.Services.AddModule<SecondModule>();

using var host = builder.Build();

await host.RunAsync();
