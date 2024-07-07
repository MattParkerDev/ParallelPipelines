using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._1Setup;

public class InstallDotnetWasmToolsStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		var result =
			await PipelineCliHelper.RunCliCommandAsync("dotnet", "workload install wasm-tools", cancellationToken);
		return [result];
	}
}
