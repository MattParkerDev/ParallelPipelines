using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Console.Modules;

[DependsOn<PowershellModule>]
public class SecondModule : IModule
{
	public async Task RunModule()
	{
		System.Console.WriteLine("🏎️ Executing SecondModule");
	}

	public void ShouldSkip()
	{
		throw new NotImplementedException();
	}
}
