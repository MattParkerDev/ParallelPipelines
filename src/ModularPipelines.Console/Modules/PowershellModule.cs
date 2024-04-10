using CliWrap;
using Domain.Entities;

namespace ModularPipelines.Console.Modules;

public class PowershellModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(2000, cancellationToken);
		//throw new ArgumentNullException();
		return null;
	}

	public bool ShouldSkip()
	{
		return false;
	}
}
