namespace Application.Attributes;

public class DependsOnAttribute<T> : Attribute where T : class
{

}

public static class AttributeExtensions
{
	public static bool DependsOn(this Type type1, Type type2)
	{
		var attributes = type1.GetCustomAttributes(typeof(DependsOnAttribute<>), true);
		return attributes.Any(a => a.GetType().GetGenericArguments().First() == type2);
	}

	public static bool HasNoDependencies(this Type type)
	{
		return !type.GetCustomAttributes(typeof(DependsOnAttribute<>), true).Any();
	}
	public static List<Type> GetDependencyTypes(this Type type)
	{
		var attributes = type.GetCustomAttributes(typeof(DependsOnAttribute<>), true);
		var types = attributes.Select(a => a.GetType().GetGenericArguments().First()).ToList();
		return types;
	}
}
