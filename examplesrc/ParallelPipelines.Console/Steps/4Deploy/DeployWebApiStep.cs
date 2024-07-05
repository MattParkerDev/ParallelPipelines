using ParallelPipelines.Console.Steps._2ProvisionAzureResources;
using ParallelPipelines.Console.Steps._3BuildAndPublish;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._4Deploy;

[DependsOnStep<PublishWebApiStep>]
[DependsOnStep<DeployBicepStep>]
public class DeployWebApiStep(IPipelineContext context) : IStep
{
	private readonly IPipelineContext _context = context;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
		return null;
	}
}
