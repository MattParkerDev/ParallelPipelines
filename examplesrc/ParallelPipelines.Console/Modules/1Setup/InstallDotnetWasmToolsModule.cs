using CliWrap;
using CliWrap.Buffered;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallDotnetWasmToolsModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		var result = await PipelineCliHelper.RunCliCommandAsync("dotnet", "workload install wasm-tools", cancellationToken);
		return [result];
	}
}
