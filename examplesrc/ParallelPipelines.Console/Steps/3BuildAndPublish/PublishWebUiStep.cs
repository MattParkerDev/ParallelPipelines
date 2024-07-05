using ParallelPipelines.Console.Steps._1Setup;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._3BuildAndPublish;

[DependsOnExp<RestoreAndBuildStep>]
[DependsOnExp<InstallDotnetWasmToolsStep>]
public class PublishWebUiStep : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
		return null;
	}
}
