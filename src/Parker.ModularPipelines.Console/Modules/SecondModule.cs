using CliWrap;
using Parker.ModularPipelines.Application.Attributes;
using Parker.ModularPipelines.Domain.Entities;

namespace Parker.ModularPipelines.Console.Modules;

[DependsOnExp<PowershellModule>]
public class SecondModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(3000, cancellationToken);
		return null;
	}

	public bool ShouldSkip()
	{
		return true;
	}
}
