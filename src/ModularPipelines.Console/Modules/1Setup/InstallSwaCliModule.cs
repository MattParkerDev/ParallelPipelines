using CliWrap;
using CliWrap.Buffered;
using Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallSwaCliModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		var forcefulCts = new CancellationTokenSource();

		await using var link = cancellationToken.Register(() =>
			forcefulCts.CancelAfter(TimeSpan.FromSeconds(3))
		);

		var result = await Cli.Wrap("npm").WithArguments("install -g @azure/static-web-apps-cli").ExecuteBufferedAsync(Console.OutputEncoding, Console.OutputEncoding, forcefulCts.Token, cancellationToken);
		return result;
	}
}
