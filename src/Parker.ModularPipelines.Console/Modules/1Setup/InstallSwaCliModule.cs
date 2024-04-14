﻿using CliWrap;
using Parker.ModularPipelines.Host.Helpers;
using Parker.ModularPipelines.Domain.Entities;

// ReSharper disable once CheckNamespace
namespace Deploy.Modules.Setup;

public class InstallSwaCliModule : IModule
{
	public async Task<CommandResult?> RunModule(CancellationToken cancellationToken)
	{
		var result = await PipelineCliHelper.RunCliCommandAsync("npm", "install -g @azure/static-web-apps-cli", cancellationToken);
		return result;
	}
	public bool ShouldSkip()
	{
		return true;
	}
}