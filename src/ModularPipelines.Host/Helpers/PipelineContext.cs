using Microsoft.Extensions.Configuration;

namespace ModularPipelines.Host.Helpers;

public class PipelineContext : IPipelineContext
{
	public IConfiguration Configuration { get; }
}

public interface IPipelineContext
{
	public IConfiguration Configuration { get; }
}
