using Application.Attributes;
using Domain.Entities;

namespace ModularPipelines.Console.Modules;

[DependsOn<PowershellModule>]
public class SecondModule : IModule
{
	public void RunModule()
	{
		throw new NotImplementedException();
	}

	public void ShouldSkip()
	{
		throw new NotImplementedException();
	}
}
