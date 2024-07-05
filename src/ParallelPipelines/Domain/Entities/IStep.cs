using CliWrap.Buffered;

namespace ParallelPipelines.Domain.Entities;

public interface IStep
{
	public Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken);

	public bool ShouldSkip()
	{
		return false;
	}
}
