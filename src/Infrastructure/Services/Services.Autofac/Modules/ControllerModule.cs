using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Mvc;

namespace Services.Autofac.Modules
{
    public class ControllerModule(params Assembly[] assemblies) : global::Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register all MVC controllers
            builder
                .RegisterAssemblyTypes(assemblies)
                .Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && t.IsPublic
                    && (
                        typeof(Controller).IsAssignableFrom(t)
                        || typeof(ControllerBase).IsAssignableFrom(t)
                        || t.Name.EndsWith("Controller")
                    )
                )
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
