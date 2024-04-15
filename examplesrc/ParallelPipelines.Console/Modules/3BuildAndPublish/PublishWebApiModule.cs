using CliWrap;
using Deploy.Modules.BuildAndPublish;
using ParallelPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Experimental.Modules.BuildAndPublish;

[DependsOnExp<RestoreAndBuildModule>]
public class PublishWebApiModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
		return null;
	}
}
