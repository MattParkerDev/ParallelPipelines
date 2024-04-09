using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Console.Modules;
using ModularPipelines.Host;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModularPipelines(builder.Configuration);
builder.Services.AddSingleton<IModule, PowershellModule>();
builder.Services.AddSingleton<IModule, SecondModule>();

using var host = builder.Build();

await host.RunAsync();
