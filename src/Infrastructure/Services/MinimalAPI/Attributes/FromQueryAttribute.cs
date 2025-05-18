namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the query string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromQueryAttribute : Attribute
{
}
