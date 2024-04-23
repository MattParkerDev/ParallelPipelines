namespace ParallelPipelines.Host.InternalHelpers;

public static class DeploymentConstants
{
	public static bool WriteDynamicLogs { get; set; }
	public static bool IsGithubActions { get; set; }
	public static bool ConsoleSupportsAnsiSequences { get; set; }
}
