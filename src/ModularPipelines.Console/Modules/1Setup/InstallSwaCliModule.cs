using CliWrap;
using CliWrap.Buffered;
using Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallSwaCliModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		var result = await Cli.Wrap("npm").WithArguments("install -g @azure/static-web-apps-cli").ExecuteBufferedAsync(cancellationToken);
		return result;
	}
}
