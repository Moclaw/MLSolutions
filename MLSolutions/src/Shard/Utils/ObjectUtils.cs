using System.Linq;
using System.Linq.Expressions;

namespace Shard.Utils;

public struct ObjectUtils
{
    public static T? Clone<T>(T obj)
    {
        if (obj == null)
            return default;
        var json = System.Text.Json.JsonSerializer.Serialize(obj, Shard.Settings.JsonSettings.DefaultOptions);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, Shard.Settings.JsonSettings.DefaultOptions);
    }

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
