using System.Linq.Expressions;

namespace MLSolutions;

/// <summary>
/// Provides extension methods for reflection-related operations.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Extracts the property names from a `new` expression.
    /// </summary>
    /// <typeparam name="T">The type of the object being inspected.</typeparam>
    /// <param name="expression">An expression representing a `new` object initialization.</param>
    /// <returns>An enumerable of property names.</returns>
    /// <exception cref="NotSupportedException">Thrown if the expression is not a valid `new` expression or contains unsupported argument types.</exception>
    public static IEnumerable<string> PropNames<T>(this Expression<Func<T, object>> expression)
    {
        if (expression.Body is not NewExpression newExp)
            throw new NotSupportedException($"[{expression}] is not a valid `new` expression!");

        return newExp.Arguments.Select(arg => arg switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression innerMember } => innerMember.Member.Name,
            _ => throw new NotSupportedException($"Argument [{arg}] is not a valid member expression!")
        });
    }

    /// <summary>
    /// Extracts the property name from a member expression.
    /// </summary>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <param name="expression">An expression representing a property or field access.</param>
    /// <returns>The name of the property or field.</returns>
    /// <exception cref="NotSupportedException">Thrown if the expression is not a valid member expression.</exception>
    public static string PropertyName<T>(this Expression<T> expression) => expression.Body switch
    {
        MemberExpression m => m.Member.Name,
        UnaryExpression { Operand: MemberExpression m } => m.Member.Name,
        _ => throw new NotSupportedException($"[{expression}] is not a valid member expression!")
    };

    /// <summary>
    /// Retrieves the generic arguments of a type if it matches a specified generic type definition.
    /// </summary>
    /// <param name="source">The type to inspect.</param>
    /// <param name="targetGeneric">The generic type definition to match.</param>
    /// <returns>An array of generic argument types, or <c>null</c> if the type does not match.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="targetGeneric"/> is not a valid generic type.</exception>
    public static Type[]? GetGenericArgumentsOfType(this Type source, Type targetGeneric)
    {
        if (!targetGeneric.IsGenericType)
            throw new ArgumentException($"{nameof(targetGeneric)} is not a valid generic type!", nameof(targetGeneric));

        for (var t = source; t != null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == targetGeneric)
                return t.GetGenericArguments();
        }
        return null;
    }

    /// <summary>
    /// Gets the underlying type of a nullable type, or the type itself if it is not nullable.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The underlying type if the type is nullable; otherwise, the type itself.</returns>
    public static Type GetUnderlyingType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    /// <summary>
    /// Determines whether a type is a value tuple type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><c>true</c> if the type is a value tuple; otherwise, <c>false</c>.</returns>
    public static bool IsValueTupleType(this Type type)
        => type.IsGenericType && type.FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) == true;
}
