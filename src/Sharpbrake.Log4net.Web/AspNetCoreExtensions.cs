#if NETSTANDARD1_4 || NETSTANDARD2_0
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using log4net;

namespace Sharpbrake.Log4net.Web
{
    public static class AspNetCoreExtensions
    {
        /// <summary>
        /// Configures the Airbrake appender with the implementation
        /// of <see cref="IHttpContextAccessor"/>.
        /// </summary>
        public static void ConfigureAirbrakeAppender(this IApplicationBuilder app, Assembly loggerRepositoryAssembly)
        {
            var appenders = LogManager.GetRepository(loggerRepositoryAssembly).GetAppenders();
            var airbrakeAppender = appenders?.FirstOrDefault(t => t.GetType() == typeof(AirbrakeAppender));
            if (airbrakeAppender != null)
                ((AirbrakeAppender) airbrakeAppender).ContextAccessor =
                    app.ApplicationServices.GetService<IHttpContextAccessor>();
        }
    }
}
#endif