using CliWrap.Buffered;
using ParallelPipelines.Application.Attributes;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Unit.Tests.TestModules;

public class TestModule1 : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}

[DependsOnExp<TestModule1>]
public class TestModule2 : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}

[DependsOnExp<TestModule1>]
public class TestModule3 : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}

[DependsOnExp<TestModule2>]
[DependsOnExp<TestModule3>]
public class TestModule4 : IModule
{
	public async Task<BufferedCommandResult?[]?> RunModule(CancellationToken cancellationToken)
	{
		await Task.Delay(1000, cancellationToken);
		return null;
	}
}
