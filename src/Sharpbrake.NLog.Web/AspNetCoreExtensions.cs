#if NETSTANDARD1_4 || NETSTANDARD2_0
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Sharpbrake.NLog.Web
{
    public static class AspNetCoreExtensions
    {
        /// <summary>
        /// Configures the Airbrake target with the implementation
        /// of <see cref="IHttpContextAccessor"/>.
        /// </summary>
        public static void ConfigureAirbrakeTarget(this IApplicationBuilder app)
        {
            var airbrakeTarget = LogManager.Configuration.AllTargets
                .FirstOrDefault(t => t.GetType() == typeof(AirbrakeTarget));

            if (airbrakeTarget != null)
                ((AirbrakeTarget) airbrakeTarget).ContextAccessor =
                    app.ApplicationServices.GetService<IHttpContextAccessor>();
        }
    }
}
#endif
