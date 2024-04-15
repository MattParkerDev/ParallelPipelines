using CliWrap;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Experimental.Modules.ProvisionAzureResources;

public class CreateResourceGroupModule(IPipelineContext context) : IModule
{
	private readonly IPipelineContext _context = context;

	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken);
		return null;
	}
}
