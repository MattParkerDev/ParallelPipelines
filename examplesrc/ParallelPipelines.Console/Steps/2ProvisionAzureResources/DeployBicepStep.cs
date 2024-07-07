using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._2ProvisionAzureResources;

[DependsOnStep<CreateResourceGroupStep>]
public class DeployBicepStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public bool ShouldSkip() => false;
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
		return null;
	}
}
