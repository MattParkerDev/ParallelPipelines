using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ParallelPipelines.Console;

public static class DependencyInjection
{
	public static IServiceCollection AddConsole(this IServiceCollection services, IConfiguration configuration)
	{
		return services;
	}
}
