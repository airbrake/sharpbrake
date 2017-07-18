#if NETSTANDARD1_4
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using Sharpbrake.Client;

namespace Sharpbrake.NLog.Web
{
    public static class AspNetCoreExtensions
    {
        /// <summary>
        /// Creates a custom Airbrake target with NLog <see cref="ConfigurationItemFactory"/>.
        /// </summary>
        public static void AddAirbrakeTarget(this IApplicationBuilder app, AirbrakeNotifier notifier)
        {
            var instanceBuilder = ConfigurationItemFactory.Default.CreateInstance;
            ConfigurationItemFactory.Default.CreateInstance =
                type => type == typeof(AirbrakeTarget)
                    ? new AirbrakeTarget(notifier, app.ApplicationServices.GetService<IHttpContextAccessor>())
                    : instanceBuilder(type);
        }
    }
}
#endif
