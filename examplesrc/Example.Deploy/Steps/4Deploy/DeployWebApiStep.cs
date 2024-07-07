using Example.Deploy.Steps._2ProvisionAzureResources;
using Example.Deploy.Steps._3BuildAndPublish;
using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._4Deploy;

[DependsOnStep<PublishWebApiStep>]
[DependsOnStep<DeployBicepStep>]
public class DeployWebApiStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
		return null;
	}
}
