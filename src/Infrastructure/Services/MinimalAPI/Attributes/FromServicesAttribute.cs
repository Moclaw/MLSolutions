namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the request service provider.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromServicesAttribute : Attribute
{
}
