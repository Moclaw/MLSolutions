using System;
using System.Linq;
using System.Reflection;


namespace Host.Attributes;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AuthorizeAttribute(string policy) : Attribute
{
    private string Policy { get; } = policy;

    public static string GetPolicyFromAttributes(MethodInfo methodInfo)
    {
        var authorizeAttributes = methodInfo.GetCustomAttributes<AuthorizeAttribute>(true);
        return authorizeAttributes.FirstOrDefault()?.Policy ?? string.Empty;
    }

    public static string GetPolicyFromAttributes(Type type)
    {
        var authorizeAttributes = type.GetCustomAttributes<AuthorizeAttribute>(true);
        return authorizeAttributes.FirstOrDefault()?.Policy ?? string.Empty;
    }
    
}