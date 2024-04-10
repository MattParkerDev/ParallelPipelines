using Application.Attributes;
using CliWrap;
using Domain.Entities;

namespace ModularPipelines.Console.Modules;

[DependsOn<PowershellModule>]
public class SecondModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(3000, cancellationToken);
		return null;
	}
}
