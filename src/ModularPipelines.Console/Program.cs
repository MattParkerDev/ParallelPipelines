using Application;
using Infrastructure;
using Microsoft.Extensions.Hosting;
using ModularPipelines.Console;
using Spectre.Console;

// var builder = Host.CreateApplicationBuilder(args);
//
// builder.Services.AddApplication(builder.Configuration);
// builder.Services.AddInfrastructure(builder.Configuration);
// builder.Services.AddConsole(builder.Configuration);
//
// using var host = builder.Build();
//
// await host.RunAsync();

Console.WriteLine("asdf");
ProgressContext progressContext;
var tasks = new List<ProgressTask>();

_ = AnsiConsole.Progress()
	.StartAsync(async ctx =>
	{
		progressContext = ctx;
		// Define tasks
		var task1 = ctx.AddTask("[green]Reticulating splines[/]");
		var task2 = ctx.AddTask("[green]Folding space[/]");
		tasks = [task1, task2];

		while (!ctx.IsFinished)
		{
			// Simulate some work
			await Task.Delay(250);

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

await Task.Delay(2000);

await Parallel.ForEachAsync(tasks, async (task, ct) =>
{
	await Task.Delay(100, ct);
	task.StopTask();
});
//tasks.ForEach(task => task.StopTask());


