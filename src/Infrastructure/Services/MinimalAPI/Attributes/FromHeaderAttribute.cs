namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the request headers.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromHeaderAttribute : Attribute { }
