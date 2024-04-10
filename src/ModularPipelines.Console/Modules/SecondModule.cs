using Application.Attributes;
using CliWrap;
using Domain.Entities;

namespace ModularPipelines.Console.Modules;

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
