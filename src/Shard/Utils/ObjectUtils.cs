using System.Linq.Expressions;

namespace Shard.Utils;

/// <summary>
/// Utility methods for object manipulation and reflection.
/// </summary>
public struct ObjectUtils
{
    /// <summary>
    /// Creates a deep clone of the specified object by serializing and deserializing it.
    /// </summary>
    /// <typeparam name="T">The type of the object to clone.</typeparam>
    /// <param name="obj">The object to clone. Can be null.</param>
    /// <returns>A deep clone of the object, or the default value of <typeparamref name="T"/> if the input is null.</returns>
    public static T? Clone<T>(T obj)
    {
        if (obj == null)
            return default;
        var json = System.Text.Json.JsonSerializer.Serialize(obj, Settings.JsonSettings.DefaultOptions);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, Settings.JsonSettings.DefaultOptions);
    }

    /// <summary>
    /// Retrieves the property path from a lambda expression.
    /// </summary>
    /// <typeparam name="TClass">The type of the class containing the property.</typeparam>
    /// <param name="expression">A lambda expression representing the property access.</param>
    /// <returns>The property path as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the expression is not a member expression.</exception>
    public static string GetPropertyPath<TClass>(Expression<Func<TClass, object>> expression) where TClass : new()
    {
        if (expression.Body is not MemberExpression)
        {
            throw new InvalidOperationException("Expression is not a member expression");
        }

        var path = expression
            .Body
            .ToString()
            .Split('.', StringSplitOptions.RemoveEmptyEntries);

        return string.Join('.', path.Skip(1));
    }
}
