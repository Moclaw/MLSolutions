using System.Reflection;
using Autofac;
using Core.Constants;
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
                    t is { IsClass: true, IsAbstract: false, IsPublic: true }
                    && (
                        typeof(Controller).IsAssignableFrom(t)
                        || typeof(ControllerBase).IsAssignableFrom(t)
                        || t.Name.EndsWith(AutofacConstants.ServiceConventions.Controller)
                    )
                )
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
