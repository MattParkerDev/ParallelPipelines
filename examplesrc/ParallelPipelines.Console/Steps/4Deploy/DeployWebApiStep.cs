using ParallelPipelines.Console.Steps._2ProvisionAzureResources;
using ParallelPipelines.Console.Steps._3BuildAndPublish;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._4Deploy;

[DependsOnStep<PublishWebApiStep>]
[DependsOnStep<DeployBicepStep>]
public class DeployWebApiStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
		return null;
	}
}
