using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Console.Steps._1Setup;

public class InstallSwaCliStep : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		var result =
			await PipelineCliHelper.RunCliCommandAsync("pnpm", "install -g @azure/static-web-apps-cli",
				cancellationToken);
		return [result];
	}
}
