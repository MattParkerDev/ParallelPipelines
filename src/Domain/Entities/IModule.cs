namespace Domain.Entities;

public interface IModule
{
	public Task RunModule(CancellationToken cancellationToken);

	public bool ShouldSkip()
	{
		return false;
	}
}
