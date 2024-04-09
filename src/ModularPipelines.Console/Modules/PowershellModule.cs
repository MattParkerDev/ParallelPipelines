using Domain.Entities;

namespace ModularPipelines.Console.Modules;

public class PowershellModule : IModule
{
	public async Task RunModule()
	{
		await Task.Delay(5000);
		System.Console.WriteLine("🏎️ Executing PowershellModule");
	}

	public void ShouldSkip()
	{
		throw new NotImplementedException();
	}
}
