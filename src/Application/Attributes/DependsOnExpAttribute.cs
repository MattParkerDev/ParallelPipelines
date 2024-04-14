using Domain.Entities;

namespace Application.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public class DependsOnExpAttribute<T> : Attribute where T : class, IModule
{
}

public static class AttributeExtensions
{
	public static bool DependsOn(this Type type1, Type type2)
	{
		var attributes = type1.GetCustomAttributes(typeof(DependsOnExpAttribute<>), true);
		return attributes.Any(a => a.GetType().GetGenericArguments().First() == type2);
	}

	public static bool HasNoDependencies(this Type type)
	{
		return !type.GetCustomAttributes(typeof(DependsOnExpAttribute<>), true).Any();
	}
	public static List<Type> GetDependencyTypes(this Type type)
	{
		var attributes = type.GetCustomAttributes(typeof(DependsOnExpAttribute<>), true);
		var types = attributes.Select(a => a.GetType().GetGenericArguments().First()).ToList();
		return types;
	}
}
