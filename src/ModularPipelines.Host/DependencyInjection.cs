using Application;
using Domain.Entities;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines.Host.Services;

namespace ModularPipelines.Host;

public static class DependencyInjection
{
	public static IServiceCollection AddModularPipelines(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddApplication(configuration);
		services.AddInfrastructure(configuration);
		services.AddHostedService<PipelineApplication>();
		services.AddSingleton<ExampleAnsiProgressService>();
		services.AddSingleton<OrchestratorService>();
		return services;
	}

	public static IServiceCollection AddModule<TModule>(this IServiceCollection services)
		where TModule : class, IModule
	{
		services.AddSingleton<TModule>();
		services.AddSingleton<ModuleContainer>(sp =>
		{
			var module = sp.GetRequiredService<TModule>();
			return new ModuleContainer(module);
		});
		services.AddSingleton<ModuleContainerProvider>();
		return services;
	}
}
