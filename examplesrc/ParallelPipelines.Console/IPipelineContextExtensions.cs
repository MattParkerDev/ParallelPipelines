namespace Deploy.Experimental;

public static class PipelineContextExtensions
{
	public static string PipelineEnvironment(this IPipelineContext context)
	{
		var environment = context.Configuration["CustomEnvironment"];

		if (string.IsNullOrEmpty(environment))
		{
			throw new InvalidOperationException("Failed to get environment name from context.");
		}

		if (environment is not AllowedEnvironments.Development and not AllowedEnvironments.Production)
		{
			throw new InvalidOperationException($"Invalid environment '{environment}'");
		}

		return environment;
	}
}

public static class AllowedEnvironments
{
	public const string Development = "dev";
	public const string Production = "prod";
}
