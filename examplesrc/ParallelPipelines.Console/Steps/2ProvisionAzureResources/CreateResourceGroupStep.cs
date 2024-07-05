using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._2ProvisionAzureResources;

public class CreateResourceGroupStep(IPipelineContext context) : IStep
{
	private readonly IPipelineContext _context = context;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken);
		return null;
	}
}
