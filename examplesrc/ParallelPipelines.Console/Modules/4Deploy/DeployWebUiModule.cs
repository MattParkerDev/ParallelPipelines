using Deploy.Experimental.Modules.ProvisionAzureResources;
using Deploy.Modules.BuildAndPublish;
using Deploy.Modules.Setup;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Deploy;

[DependsOnExp<InstallSwaCliModule>]
[DependsOnExp<DeployBicepModule>]
[DependsOnExp<PublishWebUiModule>]
public class DeployWebUiModule(IPipelineContext context) : IModule
{
	public bool ShouldSkip() => false;

	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
		return null;
	}
}
