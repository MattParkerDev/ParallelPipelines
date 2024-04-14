using Parker.ModularPipelines.Domain.Enums;

namespace Parker.ModularPipelines.Domain.Entities;

public class ModuleContainer
{
	public ModuleContainer(IModule module)
	{
		CompletedTask = new Task<ModuleContainer>(() => this);
		Module = module;
	}
	public string GetModuleName() => Module.GetType().Name;
	public bool HasCompleted { get; set; } = false;
	public Task<ModuleContainer> CompletedTask;
	public CompletionType? CompletionType { get; set; }
	public ModuleState State { get; set; } = ModuleState.Waiting;
	public IModule Module { get; set; }
	public List<ModuleContainer> Dependents { get; set; } = new();
	public List<ModuleContainer> Dependencies { get; set; } = new();

	public DateTimeOffset? StartTime { get; set; }
	public DateTimeOffset? EndTime { get; set; }
	public TimeSpan? Duration => EndTime - StartTime;
	public string? ExceptionMessage { get; set; }
}
