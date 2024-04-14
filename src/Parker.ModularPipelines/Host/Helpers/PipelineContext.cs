﻿using Microsoft.Extensions.Configuration;

namespace Parker.ModularPipelines.Host.Helpers;

public class PipelineContext(IConfiguration configuration) : IPipelineContext
{
	public IConfiguration Configuration { get; } = configuration;
}

public interface IPipelineContext
{
	public IConfiguration Configuration { get; }
}