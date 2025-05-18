namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the request body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromBodyAttribute : Attribute
{
}
