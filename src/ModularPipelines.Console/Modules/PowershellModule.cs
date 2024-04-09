using Domain.Entities;

namespace ModularPipelines.Console.Modules;

public class PowershellModule : IModule
{
	public async Task RunModule()
	{
		System.Console.WriteLine("🏎️ Executing PowershellModule");
		await Task.Delay(3000);
		System.Console.WriteLine("🏎️ Executing PowershellModule Finished");
	}

	public void ShouldSkip()
	{
		throw new NotImplementedException();
	}
}
