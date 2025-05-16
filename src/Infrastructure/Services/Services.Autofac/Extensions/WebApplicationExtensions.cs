using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Services.Autofac.Extensions
{
    /// <summary>
    /// Extensions for WebApplication to further configure Autofac
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Configure application with typical middleware using Autofac integration
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application</returns>
        public static WebApplication UseAutofacDefaults(this WebApplication app)
        {
            // Use typical middleware setup
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            return app;
        }

        /// <summary>
        /// Register a middleware that will be instantiated and resolved through the Autofac container
        /// </summary>
        /// <typeparam name="TMiddleware">The middleware type</typeparam>
        /// <param name="app">The web application</param>
        /// <returns>The web application</returns>
        public static WebApplication UseAutofacMiddleware<TMiddleware>(this WebApplication app)
        {
            app.UseMiddleware<TMiddleware>();
            return app;
        }

        /// <summary>
        /// Resolve a service from the Autofac container
        /// </summary>
        /// <typeparam name="TService">The service type</typeparam>
        /// <param name="app">The web application</param>
        /// <returns>The resolved service</returns>
        public static TService? ResolveService<TService>(this WebApplication app)
            where TService : class
        {
            return app.Services.GetService(typeof(TService)) as TService;
        }

        /// <summary>
        /// Resolve a required service from the Autofac container
        /// </summary>
        /// <typeparam name="TService">The service type</typeparam>
        /// <param name="app">The web application</param>
        /// <returns>The resolved service</returns>
        public static TService ResolveRequiredService<TService>(this WebApplication app)
            where TService : class
        {
            return (TService)app.Services.GetRequiredService(typeof(TService));
        }
    }
}
