using CliWrap;
using Deploy.Experimental.Modules.ProvisionAzureResources;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Experimental.Modules.ProvisionAzureResources;

[DependsOnExp<CreateResourceGroupModule>]
public class DeployBicepModule(IPipelineContext context) : IModule
{
	private readonly IPipelineContext _context = context;
	public bool ShouldSkip() => false;
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
		return null;
	}
}
