using Application;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularPipelines.Host;

public static class DependencyInjection
{
	public static IServiceCollection AddModularPipelines(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddApplication(configuration);
		services.AddInfrastructure(configuration);
		services.AddHostedService<PipelineApplication>();
		services.AddSingleton<ExampleAnsiProgressService>();
		return services;
	}
}
