using CliWrap;

namespace Domain.Entities;

public interface IModule
{
	public Task<CommandResult?> RunModule(CancellationToken cancellationToken);

	public bool ShouldSkip()
	{
		return false;
	}
}
