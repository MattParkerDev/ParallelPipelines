using ParallelPipelines.Console.Steps._1Setup;
using ParallelPipelines.Console.Steps._2ProvisionAzureResources;
using ParallelPipelines.Console.Steps._3BuildAndPublish;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._4Deploy;

[DependsOnExp<InstallSwaCliStep>]
[DependsOnExp<DeployBicepStep>]
[DependsOnExp<PublishWebUiStep>]
public class DeployWebUiStep : IStep
{
	public bool ShouldSkip() => false;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
		return null;
	}
}
