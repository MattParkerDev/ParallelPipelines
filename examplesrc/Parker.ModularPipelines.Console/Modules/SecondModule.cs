using CliWrap.Buffered;
using Parker.ModularPipelines.Application.Attributes;
using Parker.ModularPipelines.Domain.Entities;

namespace Parker.ModularPipelines.Console.Modules;

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
