namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using form data in the request body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromFormAttribute : Attribute
{
}
