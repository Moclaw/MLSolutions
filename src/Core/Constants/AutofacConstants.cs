namespace Core.Constants;

/// <summary>
/// Constants for Autofac registration
/// </summary>
public static class AutofacConstants
{
    /// <summary>
    /// Naming conventions for service types
    /// </summary>
    public static class ServiceConventions
    {
        /// <summary>
        /// Default service suffix
        /// </summary>
        public const string Service = "Service";

        /// <summary>
        /// Repository suffix
        /// </summary>
        public const string Repository = "Repository";

        /// <summary>
        /// Manager suffix
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>
        /// Factory suffix
        /// </summary>
        public const string Factory = "Factory";

        /// <summary>
        /// Provider suffix
        /// </summary>
        public const string Provider = "Provider";

        /// <summary>
        /// Controller suffix
        /// </summary>
        public const string Controller = "Controller";

        /// <summary>
        /// Interface prefix
        /// </summary>
        public const string InterfacePrefix = "I";

        /// <summary>
        /// Implementation suffix to remove when matching interfaces
        /// </summary>
        public const string ImplSuffix = "Impl";

        /// <summary>
        /// Implementation suffix to remove when matching interfaces
        /// </summary>
        public const string ImplementationSuffix = "Implementation";

        /// <summary>
        /// Get all default service type suffixes
        /// </summary>
        public static string[] GetDefaultSuffixes() =>
            [Service, Repository, Manager, Factory, Provider];
    }

    /// <summary>
    /// Registration lifetimes
    /// </summary>
    public static class Lifetimes
    {
        /// <summary>
        /// Singleton lifetime
        /// </summary>
        public const string Singleton = "Singleton";

        /// <summary>
        /// Scoped lifetime
        /// </summary>
        public const string Scoped = "Scoped";

        /// <summary>
        /// Transient lifetime
        /// </summary>
        public const string Transient = "Transient";
    }
}
