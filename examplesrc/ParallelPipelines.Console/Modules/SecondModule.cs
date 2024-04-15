using CliWrap.Buffered;
using ParallelPipelines.Application.Attributes;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Modules;

[DependsOnExp<PowershellModule>]
public class SecondModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(3000, cancellationToken);
		return null;
	}

	public bool ShouldSkip()
	{
		return true;
	}
}
