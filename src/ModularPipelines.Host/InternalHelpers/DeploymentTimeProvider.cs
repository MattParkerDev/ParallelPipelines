namespace ModularPipelines.Host.InternalHelpers;

internal static class DeploymentTimeProvider
{
	internal static DateTimeOffset? DeploymentStartTime;
	internal static DateTimeOffset? DeploymentEndTime;
	internal static TimeSpan? DeploymentDuration => DeploymentEndTime - DeploymentStartTime;
}
