namespace ParallelPipelines.Domain.Enums;

public enum CompletionType
{
	Success,
	PrevSuccess,
	Failure,
	Skipped,
	Cancelled
}

public enum StepState
{
	Waiting,
	Running,
	Completed
}

// WIP
public enum SkipHandling
{
	ContinueAllDependents, // all dependents will continue
	SkipAllDependents, // all dependents recursively will be skipped
	FailAll // all dependents will be failed, causing pipeline failure
}
