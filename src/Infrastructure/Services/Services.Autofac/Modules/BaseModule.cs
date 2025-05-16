using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Autofac;
using Autofac.Builder;

namespace Services.Autofac.Modules
{
    /// <summary>
    /// Base Autofac module that provides common functionality for all modules
    /// </summary>
    public abstract class BaseModule : global::Autofac.Module
    {
        protected readonly Assembly[] Assemblies;

        protected BaseModule(params Assembly[] assemblies)
        {
            Assemblies = assemblies.Length == 0 
                ? new[] { Assembly.GetCallingAssembly() } 
                : assemblies;
        }
        
        /// <summary>
        /// Override to provide custom module registration logic
        /// </summary>
        /// <param name="builder">The container builder</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }

        /// <summary>
        /// Register a type as itself and all its interfaces
        /// </summary>
        /// <typeparam name="TType">The type to register</typeparam>
        /// <param name="builder">The container builder</param>
        /// <param name="lifetime">The lifetime of the registration (singleton, scoped or transient)</param>
        protected void RegisterType<TType>(ContainerBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TType : class
        {
            var registration = builder.RegisterType<TType>().AsSelf().AsImplementedInterfaces();

            ApplyLifetime(registration, lifetime);
        }

        /// <summary>
        /// Register an instance as itself and all its interfaces
        /// </summary>
        /// <typeparam name="TType">The type to register</typeparam>
        /// <param name="builder">The container builder</param>
        /// <param name="instance">The instance to register</param>
        protected void RegisterInstance<TType>(ContainerBuilder builder, TType instance)
            where TType : class
        {
            builder.RegisterInstance(instance).AsSelf().AsImplementedInterfaces().SingleInstance();
        }        /// <summary>
        /// Apply the correct lifetime to a registration
        /// </summary>
        /// <param name="registration">The registration to apply the lifetime to</param>
        /// <param name="lifetime">The lifetime to apply</param>
        protected void ApplyLifetime<T>(
            IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration,
            ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    registration.SingleInstance();
                    break;
                case ServiceLifetime.Scoped:
                    registration.InstancePerLifetimeScope();
                    break;
                case ServiceLifetime.Transient:
                    registration.InstancePerDependency();
                    break;
            }
        }        /// <summary>
        /// Apply the correct lifetime to a scanning registration
        /// </summary>
        /// <param name="registration">The registration to apply the lifetime to</param>
        /// <param name="lifetime">The lifetime to apply</param>
        protected void ApplyLifetime(
            global::Autofac.Builder.IRegistrationBuilder<object, global::Autofac.Features.Scanning.ScanningActivatorData, global::Autofac.Builder.DynamicRegistrationStyle> registration,
            ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    registration.SingleInstance();
                    break;
                case ServiceLifetime.Scoped:
                    registration.InstancePerLifetimeScope();
                    break;
                case ServiceLifetime.Transient:
                    registration.InstancePerDependency();
                    break;
            }
        }
    }
}