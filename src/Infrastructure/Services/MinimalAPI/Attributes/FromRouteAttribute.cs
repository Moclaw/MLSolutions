namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using route-data from the current request.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromRouteAttribute : Attribute
{
}
