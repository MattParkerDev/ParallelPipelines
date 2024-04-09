namespace ModularPipelines.Host.Services;

public class OrchestratorService
{
	public async Task ExecuteAsync()
	{
		Console.WriteLine("Executing OrchestratorService");
		await Task.Delay(1000);
		Console.WriteLine("OrchestratorService Complete");
	}
}
