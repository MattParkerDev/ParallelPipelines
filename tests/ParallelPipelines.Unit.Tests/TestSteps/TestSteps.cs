using CliWrap.Buffered;
using ParallelPipelines.Application.Attributes;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Unit.Tests.TestSteps;

public class TestStep1 : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}

[DependsOnStep<TestStep1>]
public class TestStep2 : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}

[DependsOnStep<TestStep1>]
public class TestStep3 : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}

[DependsOnStep<TestStep2>]
[DependsOnStep<TestStep3>]
public class TestStep4 : IStep
{
	public async Task<BufferedCommandResult?[]?> RunStep(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}
