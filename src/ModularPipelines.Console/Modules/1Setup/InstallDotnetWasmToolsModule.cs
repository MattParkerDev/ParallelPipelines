using CliWrap;
using CliWrap.Buffered;
using Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallDotnetWasmToolsModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		var result = await Cli.Wrap("dotnet").WithArguments("workload install wasm-tools").ExecuteBufferedAsync(cancellationToken);
		return result;
	}
}
