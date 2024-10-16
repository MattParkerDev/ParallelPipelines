﻿using ParallelPipelines.Domain.Entities;

namespace ParallelPipelines.Application.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public class DependsOnStepAttribute<T> : Attribute where T : class, IStep
{
}

public static class AttributeExtensions
{
	public static bool DependsOn(this Type type1, Type type2)
	{
		var attributes = type1.GetCustomAttributes(typeof(DependsOnStepAttribute<>), true);
		return attributes.Any(a => a.GetType().GetGenericArguments().First() == type2);
	}

	public static bool HasNoDependencies(this Type type)
	{
		return !type.GetCustomAttributes(typeof(DependsOnStepAttribute<>), true).Any();
	}
	public static List<Type> GetDependencyTypes(this Type type)
	{
		var attributes = type.GetCustomAttributes(typeof(DependsOnStepAttribute<>), true);
		var types = attributes.Select(a => a.GetType().GetGenericArguments().First()).ToList();
		return types;
	}
}
