using Core.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.External.SmsService;
using Services.External.SmtpService;

namespace Services.External
{
    public static class ServicesRegister
    {
        public static void AddSmtpService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpConfiguration>(configuration.GetSection(SmtpConfiguration.SectionName));

            services.AddTransient<ISmtpServices, SmtpServices>();
        }

        public static void AddSmsService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmsConfiguration>(configuration.GetSection(SmsConfiguration.SectionName));
            services.AddTransient<ISmsServices, SmsServices>();
        }
    }
}
