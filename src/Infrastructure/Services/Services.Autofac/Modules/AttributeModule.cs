using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.Autofac.Modules
{
    /// <summary>
    /// Module for registering services based on attributes
    /// </summary>
    public class AttributeModule : BaseModule
    {
        /// <summary>
        /// Creates a new attribute module
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for attributed types</param>
        public AttributeModule(params Assembly[] assemblies) : base(assemblies)
        {
        }

        /// <summary>
        /// Load the attribute-based registrations
        /// </summary>
        /// <param name="builder">The container builder</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            foreach (var assembly in Assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract)
                    {
                        continue;
                    }

                    // Register based on attributes
                    if (type.GetCustomAttribute<SingletonAttribute>() != null)
                    {
                        RegisterTypeWithLifetime(builder, type, Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);
                    }
                    else if (type.GetCustomAttribute<ScopedAttribute>() != null)
                    {
                        RegisterTypeWithLifetime(builder, type, Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
                    }
                    else if (type.GetCustomAttribute<TransientAttribute>() != null)
                    {
                        RegisterTypeWithLifetime(builder, type, Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient);
                    }
                }
            }
        }

        private void RegisterTypeWithLifetime(ContainerBuilder builder, Type type, Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime)
        {
            var registration = builder.RegisterType(type).AsImplementedInterfaces();
            
            // Also register as self if marked with the RegistrationTarget attribute
            if (type.GetCustomAttribute<RegisterAsSelfAttribute>() != null)
            {
                registration.AsSelf();
            }

            ApplyLifetime(registration, lifetime);
        }
    }

    /// <summary>
    /// Attribute to indicate that a type should also be registered as itself
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsSelfAttribute : Attribute { }
}