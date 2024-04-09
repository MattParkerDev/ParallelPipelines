namespace Application.Attributes;

public class DependsOnAttribute<T> : Attribute where T : class
{

}

public static class AttributeExtensions
{
	public static bool DependsOn<T>(this Type type) where T : class
	{
		return type.GetCustomAttributes(typeof(DependsOnAttribute<T>), true).Any();
	}

	public static bool HasNoDependencies(this Type type)
	{
		return !type.GetCustomAttributes(typeof(DependsOnAttribute<>), true).Any();
	}
	public static object[] GetDependencies(this Type type)
	{
		var attributes = type.GetCustomAttributes(typeof(DependsOnAttribute<>), true);
		return attributes;
	}
}
