using Example.Deploy.Steps._1Setup;
using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._3BuildAndPublish;

[DependsOnStep<RestoreAndBuildStep>]
[DependsOnStep<InstallDotnetWasmToolsStep>]
public class PublishWebUiStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
		return null;
	}
}
