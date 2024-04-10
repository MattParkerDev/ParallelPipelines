using Domain.Entities;

namespace ModularPipelines.Console.Modules;

public class PowershellModule : IModule
{
	public async Task RunModule(CancellationToken cancellationToken)
	{
		System.Console.WriteLine("🏎️ Executing PowershellModule");
		await Task.Delay(2000, cancellationToken);
		//throw new ArgumentNullException();
		System.Console.WriteLine("🏎️ Executing PowershellModule Finished");
	}

	public bool ShouldSkip()
	{
		return false;
	}
}
