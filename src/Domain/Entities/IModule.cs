namespace Domain.Entities;

public interface IModule
{
	public Task RunModule();
	public void ShouldSkip();
}
