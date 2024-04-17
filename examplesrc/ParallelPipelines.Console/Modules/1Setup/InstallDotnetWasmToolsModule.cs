using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallDotnetWasmToolsModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		var result =
			await PipelineCliHelper.RunCliCommandAsync("dotnet", "workload install wasm-tools", cancellationToken);
		return [result];
	}
}
