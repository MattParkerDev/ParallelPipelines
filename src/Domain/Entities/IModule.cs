namespace Domain.Entities;

public interface IModule
{
	public void RunModule();
	public void ShouldSkip();
}
