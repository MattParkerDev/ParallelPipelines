using Spectre.Console;

namespace ParallelPipelines.Host.Services;

public class ExampleAnsiProgressService
{
	public async Task DoWork(CancellationToken cancellationToken)
	{
		AnsiConsole.WriteLine("Starting Ansi Console Progress Work");
		ProgressContext progressContext;
		var tasks = new List<ProgressTask>();

		await AnsiConsole.Progress()
			.StartAsync(async ctx =>
			{
				progressContext = ctx;
				// Define tasks
				var task1 = ctx.AddTask("[green]Reticulating splines[/]");
				var task2 = ctx.AddTask("[green]Folding space[/]");
				tasks = [task1, task2];

				while (!ctx.IsFinished && cancellationToken.IsCancellationRequested is false)
				{
					// Simulate some work
					await Task.Delay(250, cancellationToken);

					// Increment
					if (task1.IsFinished is false)
					{
						task1.Increment(1.5);
					}

					if (task2.IsFinished is false)
					{
						task2.Increment(1.0);
					}
					//task1.Increment(100);
					//task1.StopTask();
				}
			});
	}
}
