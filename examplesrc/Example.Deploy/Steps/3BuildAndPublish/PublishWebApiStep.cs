using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._3BuildAndPublish;

[DependsOnStep<RestoreAndBuildStep>]
public class PublishWebApiStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
		return null;
	}
}
