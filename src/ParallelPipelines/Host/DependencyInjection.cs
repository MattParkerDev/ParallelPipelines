using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;
using ParallelPipelines.Host.Services;
using ParallelPipelines.Application;
using ParallelPipelines.Host.Services.GithubActions;
using ParallelPipelines.Infrastructure;
using Spectre.Console;

namespace ParallelPipelines.Host;

public static class DependencyInjection
{
	public static IServiceCollection AddParallelPipelines(this IServiceCollection services,
		IConfiguration configuration, Action<PipelineConfig>? action = null)
	{
		services.AddApplication(configuration);
		services.AddInfrastructure(configuration);
		services.AddHostedService<PipelineApplication>();
		services.AddSingleton<OrchestratorService>();
		services.AddSingleton<PostStepService>();
		services.AddSingleton<ModuleContainerProvider>();
		services.AddSingleton<ConsoleRenderer>();

		services.AddSingleton<GithubActionTableSummaryService>();
		services.AddSingleton<GithubActionGanttSummaryService>();


		if (action is not null)
		{
			services.Configure(action);
		}

		Console.OutputEncoding = Encoding.UTF8;
		services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);

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
