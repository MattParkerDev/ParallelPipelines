using CliWrap;
using Parker.ModularPipelines.Domain.Entities;

namespace Parker.ModularPipelines.Console.Modules;

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
		return true;
	}
}
