using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._2ProvisionAzureResources;

[DependsOnExp<CreateResourceGroupStep>]
public class DeployBicepStep(IPipelineContext context) : IStep
{
	private readonly IPipelineContext _context = context;
	public bool ShouldSkip() => false;
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
		return null;
	}
}
