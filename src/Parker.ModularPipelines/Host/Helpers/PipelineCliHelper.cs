using CliWrap;
using CliWrap.Buffered;

namespace Parker.ModularPipelines.Host.Helpers;

public static class PipelineCliHelper
{
	public static async Task<BufferedCommandResult?> RunCliCommandAsync(string targetFilePath, string arguments,
		CancellationToken cancellationToken)
	{
		var command = Cli.Wrap(targetFilePath).WithArguments(arguments);
		var result = await command.ExecuteBufferedAsync(cancellationToken);
		if (result?.IsSuccess is false)
		{
			throw new InvalidOperationException(result.StandardError + result.StandardOutput);
		}
		return result;
	}

	public static async Task<BufferedCommandResult?> ExecuteBufferedAsync(this Command command,
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
