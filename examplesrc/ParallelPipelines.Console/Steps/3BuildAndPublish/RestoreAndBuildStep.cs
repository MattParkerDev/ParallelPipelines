using ParallelPipelines.Console.Steps._1Setup;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._3BuildAndPublish;

[DependsOnExp<InstallDotnetWasmToolsStep>]
public class RestoreAndBuildStep : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
		return null;
	}
}
