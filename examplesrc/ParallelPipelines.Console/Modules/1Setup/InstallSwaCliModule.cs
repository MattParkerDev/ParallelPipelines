using CliWrap;
using CliWrap.Buffered;
using ParallelPipelines.Domain.Entities;
using ParallelPipelines.Host.Helpers;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallSwaCliModule : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		var result = await PipelineCliHelper.RunCliCommandAsync("npm", "install -g @azure/static-web-apps-cli", cancellationToken);
		return [result];
	}
	public bool ShouldSkip()
	{
		return true;
	}
}
