namespace Domain.Entities;

public interface IModule
{
	public Task RunModule(CancellationToken cancellationToken);
	public void ShouldSkip();
}
