using Example.Deploy.Steps._1Setup;
using Example.Deploy.Steps._2ProvisionAzureResources;
using Example.Deploy.Steps._3BuildAndPublish;
using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._4Deploy;

[DependsOnStep<InstallSwaCliStep>]
[DependsOnStep<DeployBicepStep>]
[DependsOnStep<PublishWebUiStep>]
public class DeployWebUiStep : IStep
{
	public bool ShouldSkip() => false;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(12), cancellationToken);
		return null;
	}
}
