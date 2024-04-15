using System.Globalization;

namespace Deploy.Experimental;

public static class DeploymentTimeProvider
{
	private static DateTimeOffset? _deploymentStartTime;

	private static DateTimeOffset GetDeploymentStartTime()
	{
		_deploymentStartTime ??= DateTimeOffset.UtcNow;
		return _deploymentStartTime.Value;
	}

	public static string GetDeploymentStartTimeString()
	{
		var timeString = GetDeploymentStartTime().ToString("yyyy-MM-ddThh.mm.ss", CultureInfo.InvariantCulture);
		return timeString;
	}
}

public static class DeploymentConstants
{
	public const string AppName = "statusapp";
	public const string SolutionFileName = "StatusApp.sln";
}
