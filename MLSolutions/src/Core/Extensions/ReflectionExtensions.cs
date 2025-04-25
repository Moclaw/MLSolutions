using System.Linq.Expressions;

namespace MLSolutions;

public static class ReflectionExtensions
{
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

    public static string PropertyName<T>(this Expression<T> expression)
    {
        return expression.Body switch
        {
            MemberExpression m => m.Member.Name,
            UnaryExpression { Operand: MemberExpression m } => m.Member.Name,
            _ => throw new NotSupportedException($"[{expression}] is not a valid member expression!")
        };
    }

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

    public static Type GetUnderlyingType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    public static bool IsValueTupleType(this Type type)
        => type.IsGenericType && type.FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) == true;
}
