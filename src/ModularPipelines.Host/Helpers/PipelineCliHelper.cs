using CliWrap;
using CliWrap.Buffered;

namespace ModularPipelines.Host.Helpers;

public static class PipelineCliHelper
{
	public static async Task<CommandResult?> RunCliCommandAsync(string targetFilePath, string arguments, CancellationToken cancellationToken)
	{
		var command = Cli.Wrap(targetFilePath).WithArguments(arguments);
		var result = await command.ExecuteBufferedAsync(cancellationToken);
		return result;
	}

	public static async Task<CommandResult?> ExecuteBufferedAsync(this Command command, CancellationToken cancellationToken)
	{
		var forcefulCts = new CancellationTokenSource();

		await using var link = cancellationToken.Register(() =>
			forcefulCts.CancelAfter(TimeSpan.FromSeconds(3))
		);

		var result = await command.ExecuteBufferedAsync(Console.OutputEncoding, Console.OutputEncoding, forcefulCts.Token, cancellationToken);
		return result;
	}
}
