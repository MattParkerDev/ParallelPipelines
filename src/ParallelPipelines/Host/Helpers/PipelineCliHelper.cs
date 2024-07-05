using CliWrap;
using CliWrap.Buffered;
using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Host.Helpers;

public static class PipelineCliHelper
{
	public static async Task<BufferedCommandResult?> RunCliCommandInDirectoryAsync(string workingDirectory, string targetFilePath, string arguments,
		CancellationToken cancellationToken)
	{
		var command = Cli.Wrap(targetFilePath).WithWorkingDirectory(workingDirectory).WithArguments(arguments);
		var result = await RunCliCommandAsync(command, cancellationToken);
		return result;
	}

	public static async Task<BufferedCommandResult?> RunCliCommandAsync(string targetFilePath, string arguments,
		CancellationToken cancellationToken)
	{
		if (cancellationToken == CancellationToken.None)
		{
			throw new InvalidOperationException($"Please pass the cancellation token from {nameof(IStep.RunStep)} to {nameof(RunCliCommandAsync)}!");
		}
		var command = Cli.Wrap(targetFilePath).WithArguments(arguments);
		var result = await RunCliCommandAsync(command, cancellationToken);
		return result;
	}

	private static async Task<BufferedCommandResult?> RunCliCommandAsync(Command command, CancellationToken cancellationToken)
	{
		var result = await command.ExecuteBufferedWithGracefulCancelAsync(cancellationToken);
		if (result?.IsSuccess is false)
		{
			throw new InvalidOperationException(result.StandardError + result.StandardOutput);
		}
		return result;
	}

	private static async Task<BufferedCommandResult?> ExecuteBufferedWithGracefulCancelAsync(this Command command,
		CancellationToken cancellationToken)
	{
		var forcefulCts = new CancellationTokenSource();

		await using var link = cancellationToken.Register(() =>
			forcefulCts.CancelAfter(TimeSpan.FromSeconds(3))
		);

		var result = await command.WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(Console.OutputEncoding, Console.OutputEncoding,
			forcefulCts.Token, cancellationToken);
		return result;
	}
}
