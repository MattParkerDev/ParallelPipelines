using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Host.Helpers;

public class PipelineContext(IConfiguration configuration, IOptions<PipelineConfig> pipelineConfig) : IPipelineContext
{
	public IConfiguration Configuration { get; } = configuration;
	private readonly PipelineConfig _pipelineConfig = pipelineConfig.Value;
	private string? _pipelineEnvironment;

	public string GetPipelineEnvironment() => _pipelineEnvironment ?? throw new InvalidOperationException("The pipeline environment has not been set - this is a ParallelPipelines bug");

	public void ValidateandPopulatePipelineEnvironment()
	{
		var environment = Configuration["ParallelPipelinesEnvironment"];

		if (string.IsNullOrEmpty(environment))
		{
			throw new InvalidOperationException("Failed to get environment name from context.");
		}

		if (_pipelineConfig.AllowedEnvironmentNames?.Contains(environment) is false)
		{
			throw new InvalidOperationException($"Invalid environment '{environment}', allowed environments ({_pipelineConfig.AllowedEnvironmentNames.Count}) are: {string.Join(", ", _pipelineConfig.AllowedEnvironmentNames.Select(s => "'" + s + "'"))}");
		}

		_pipelineEnvironment = environment;
	}
}

public interface IPipelineContext
{
	public IConfiguration Configuration { get; }
	internal void ValidateandPopulatePipelineEnvironment();
	public string GetPipelineEnvironment();
}
