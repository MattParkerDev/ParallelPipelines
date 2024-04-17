using Deploy.Modules.Setup;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.BuildAndPublish;

[DependsOnExp<RestoreAndBuildModule>]
[DependsOnExp<InstallDotnetWasmToolsModule>]
public class PublishWebUiModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
		return null;
	}
}
