namespace Domain.Entities;

public class ModuleContainer
{
	public ModuleContainer(IModule module)
	{
		CompletedSuccessfullyTask = new Task<ModuleContainer>(() => this);
		Module = module;
	}

	public bool? HasCompletedSuccessfully { get; set; }
	public Task<ModuleContainer> CompletedSuccessfullyTask;
	public IModule Module { get; set; }
	public List<ModuleContainer> Dependents { get; set; } = new();
}
