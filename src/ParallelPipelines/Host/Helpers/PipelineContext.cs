using Microsoft.Extensions.Configuration;

namespace ParallelPipelines.Host.Helpers;

public class PipelineContext(IConfiguration configuration) : IPipelineContext
{
	public IConfiguration Configuration { get; } = configuration;
}

public interface IPipelineContext
{
	public IConfiguration Configuration { get; }
}
