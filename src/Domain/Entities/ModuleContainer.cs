namespace Domain.Entities;

public class ModuleContainer<T> : IModuleContainer where T : IModule
//public class ModuleContainer<T> where T : IModule
{
	public bool HasCompletedSuccessfully { get; set; }
	public IModule Module { get; set; }

	public ModuleContainer(T module)
	{
		Module = module;
	}


}
