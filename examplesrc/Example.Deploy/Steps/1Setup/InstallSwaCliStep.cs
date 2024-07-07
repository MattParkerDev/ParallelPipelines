using ParallelPipelines.Domain.Entities;

namespace Example.Deploy.Steps._1Setup;

public class InstallSwaCliStep(IPipelineContext pipelineContext) : IStep
{
	private readonly IPipelineContext _pipelineContext = pipelineContext;

	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		var result =
			await PipelineCliHelper.RunCliCommandAsync("pnpm", "install -g @azure/static-web-apps-cli",
				cancellationToken);
		return [result];
	}
}
