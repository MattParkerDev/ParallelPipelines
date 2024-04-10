using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallDotnetWasmToolsModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		var forcefulCts = new CancellationTokenSource();

		await using var link = cancellationToken.Register(() =>
			forcefulCts.CancelAfter(TimeSpan.FromSeconds(3))
		);
		var result = await Cli.Wrap("dotnet").WithArguments("workload install wasm-tools").ExecuteBufferedAsync(Console.OutputEncoding, Console.OutputEncoding, forcefulCts.Token, cancellationToken);
		return result;
	}
}
