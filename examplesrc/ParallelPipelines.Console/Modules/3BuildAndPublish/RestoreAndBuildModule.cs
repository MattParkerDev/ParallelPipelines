using CliWrap;
using Deploy.Experimental;
using Deploy.Modules.Setup;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.BuildAndPublish;

[DependsOnExp<InstallDotnetWasmToolsModule>]
public class RestoreAndBuildModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
		return null;
	}
}
