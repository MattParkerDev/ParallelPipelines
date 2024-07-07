using Example.Deploy.Steps._1Setup;
using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._3BuildAndPublish;

[DependsOnStep<InstallDotnetWasmToolsStep>]
public class RestoreAndBuildStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
		return null;
	}
}
