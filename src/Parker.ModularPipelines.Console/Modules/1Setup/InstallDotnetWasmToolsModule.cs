using CliWrap;
using Parker.ModularPipelines.Domain.Entities;
using Parker.ModularPipelines.Host.Helpers;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallDotnetWasmToolsModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		var result = await PipelineCliHelper.RunCliCommandAsync("dotnet", "workload install wasm-tools", cancellationToken);
		return result;
	}
}
