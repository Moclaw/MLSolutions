namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter should be bound from the dependency injection container
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class FromServicesAttribute : Attribute
{
}
