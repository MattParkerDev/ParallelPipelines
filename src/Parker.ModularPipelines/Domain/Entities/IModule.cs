using CliWrap;
using CliWrap.Buffered;

namespace Parker.ModularPipelines.Domain.Entities;

public interface IModule
{
	public Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken);

	public bool ShouldSkip()
	{
		return false;
	}
}
