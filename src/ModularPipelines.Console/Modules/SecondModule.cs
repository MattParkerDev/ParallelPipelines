using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Console.Modules;

[DependsOn<PowershellModule>]
public class SecondModule : IModule
{
	public async Task RunModule(CancellationToken cancellationToken)
	{
		System.Console.WriteLine("🏎️ Executing SecondModule");
		await Task.Delay(3000, cancellationToken);
		System.Console.WriteLine("🏎️ Executing SecondModule Finished");
	}

	public void ShouldSkip()
	{
		throw new NotImplementedException();
	}
}
