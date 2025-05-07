using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Host.Filters
{
    internal class StartupFilter(string applicationName) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<StartupFilter>>();
                var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

                if (serverAddressesFeature != null)
                {
                    foreach (var address in serverAddressesFeature.Addresses)
                    {
                        logger.LogInformation("Application '{ApplicationName}' is running at URL: {Url}", applicationName, address);
                    }
                }
                else
                {
                    logger.LogWarning("Unable to retrieve application URL.");
                }

                next(app);
            };
        }
    }
}
