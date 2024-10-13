using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
		services.AddSingleton<StepContainerProvider>();
		services.AddSingleton<ConsoleRenderer>();

		services.AddSingleton<GithubActionTableSummaryService>();
		services.AddSingleton<GithubActionGanttSummaryService>();


		if (action is not null)
		{
			services.Configure(action);
		}
		else
		{
			services.AddOptions<PipelineConfig>();
		}

		Console.OutputEncoding = Encoding.UTF8;
		services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);

		services.AddSingleton<IPipelineContext, PipelineContext>();
		return services;
	}

	public static IServiceCollection AddStep<TStep>(this IServiceCollection services)
		where TStep : class, IStep
	{
		services.AddSingleton<TStep>();
		services.AddSingleton<StepContainer>(sp =>
		{
			var step = sp.GetRequiredService<TStep>();
			return new StepContainer(step);
		});
		return services;
	}
}
