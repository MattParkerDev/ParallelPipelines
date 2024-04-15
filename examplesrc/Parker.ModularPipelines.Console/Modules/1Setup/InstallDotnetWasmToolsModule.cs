using CliWrap;
using CliWrap.Buffered;
using Parker.ModularPipelines.Domain.Entities;
using Parker.ModularPipelines.Host.Helpers;

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
