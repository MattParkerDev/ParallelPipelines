using CliWrap.Buffered;
using ParallelPipelines.Domain.Enums;

namespace ParallelPipelines.Domain.Entities;

public class StepContainer
{
	public StepContainer(IStep step)
	{
		CompletedTask = new Task<StepContainer>(() => this);
		Step = step;
	}
	public string GetStepName() => Step.GetType().Name;
	public bool HasCompleted { get; set; } = false;
	public Task<StepContainer> CompletedTask;
	public CompletionType? CompletionType { get; set; }
	public StepState State { get; set; } = StepState.Waiting;
	public IStep Step { get; set; }
	public List<StepContainer> Dependents { get; set; } = new();
	public List<StepContainer> Dependencies { get; set; } = new();

	public DateTimeOffset? StartTime { get; set; }
	public DateTimeOffset? EndTime { get; set; }
	public TimeSpan? Duration => EndTime - StartTime;
	public Exception? Exception { get; set; }
	public BufferedCommandResult?[]? CliCommandResults { get; set; }
}
