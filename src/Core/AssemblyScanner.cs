using System.Reflection;

namespace Core;
public class AssemblyScanner
{
    public static IEnumerable<Type> FindTypesInNamespace(Assembly assembly, string namespaceName) => assembly.GetTypes()
            .Where(t => t.IsClass && t.Namespace == namespaceName);

    public static IEnumerable<Type> FindTypesInNamespace(Assembly assembly, Func<Type, bool> predicate) => assembly.GetTypes()
            .Where(predicate);
}