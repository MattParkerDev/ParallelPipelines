using CliWrap;
using CliWrap.Buffered;
using Deploy.Experimental;
using Deploy.Experimental.Modules.ProvisionAzureResources;
using Deploy.Experimental.Modules.BuildAndPublish;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Deploy;

[DependsOnExp<PublishWebApiModule>]
[DependsOnExp<DeployBicepModule>]
public class DeployWebApiModule(IPipelineContext context) : IModule
{
	private readonly IPipelineContext _context = context;

	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
		return null;
	}
}
