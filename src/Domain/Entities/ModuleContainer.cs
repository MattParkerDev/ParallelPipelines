namespace Domain.Entities;

public class ModuleContainer
{
	public ModuleContainer(IModule module)
	{
		Module = module;
	}

	public bool HasCompletedSuccessfully { get; set; }
	public IModule Module { get; set; }
}
