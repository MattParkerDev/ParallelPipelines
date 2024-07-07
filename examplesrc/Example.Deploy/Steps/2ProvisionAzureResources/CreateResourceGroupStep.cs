using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._2ProvisionAzureResources;

public class CreateResourceGroupStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken);
		return null;
	}
}
