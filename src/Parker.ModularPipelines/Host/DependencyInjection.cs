using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parker.ModularPipelines.Application;
using Parker.ModularPipelines.Domain.Entities;
using Parker.ModularPipelines.Host.Helpers;
using Parker.ModularPipelines.Host.Services;
using Parker.ModularPipelines.Infrastructure;

namespace Parker.ModularPipelines.Host;

public static class DependencyInjection
{
	public static IServiceCollection AddModularPipelines(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddApplication(configuration);
		services.AddInfrastructure(configuration);
		services.AddHostedService<PipelineApplication>();
		services.AddSingleton<ExampleAnsiProgressService>();
		services.AddSingleton<OrchestratorService>();
		services.AddSingleton<ModuleContainerProvider>();
		services.AddSingleton<IPipelineContext, PipelineContext>(sp => new PipelineContext(configuration));
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
		return services;
	}
}
