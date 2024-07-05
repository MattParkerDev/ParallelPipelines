using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._1Setup;

public class InstallDotnetWasmToolsStep : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		var result =
			await PipelineCliHelper.RunCliCommandAsync("dotnet", "workload install wasm-tools", cancellationToken);
		return [result];
	}
}
