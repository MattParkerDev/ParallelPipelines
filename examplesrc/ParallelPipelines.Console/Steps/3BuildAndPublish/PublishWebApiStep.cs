using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._3BuildAndPublish;

[DependsOnExp<RestoreAndBuildStep>]
public class PublishWebApiStep : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
		return null;
	}
}
