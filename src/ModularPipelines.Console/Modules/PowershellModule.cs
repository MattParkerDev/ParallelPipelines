using Domain.Entities;

namespace ModularPipelines.Console.Modules;

public class PowershellModule : IModule
{
	public async Task RunModule()
	{
		System.Console.WriteLine("🏎️ Executing PowershellModule");
	}

	public void ShouldSkip()
	{
		throw new NotImplementedException();
	}
}
