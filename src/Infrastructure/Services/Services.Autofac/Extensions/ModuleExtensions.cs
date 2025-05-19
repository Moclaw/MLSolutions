using Autofac;
using System.Reflection;

namespace Services.Autofac.Extensions
{
    public static class ModuleExtensions
    {
        /// <summary>
        /// Register all modules in the specified assemblies
        /// </summary>
        /// <param name="builder">The container builder</param>
        /// <param name="assemblies">Assemblies to scan for modules</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterModules(
            this ContainerBuilder builder,
            params Assembly[] assemblies
        )
        {
            var moduleTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(global::Autofac.Module).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (
                var module in moduleTypes.Select(moduleType =>
                    (global::Autofac.Module)System.Activator.CreateInstance(moduleType)!
                )
            )
            {
                // Register the module
                builder.RegisterModule(module);
            }

            return builder;
        }
    }
}
