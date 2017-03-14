using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Sharpbrake.Client;

namespace Sharpbrake.Http.Middleware
{
    public static class AirbrakeExtension
    {
        /// <summary>
        /// Adds the Airbrake middleware to the request pipeline.
        /// Creates the <see cref="AirbrakeNotifier"/> instance and initializes it with provided config.
        /// </summary>
        public static IApplicationBuilder UseAirbrake(this IApplicationBuilder app, IConfiguration config)
        {
            return app.UseMiddleware<AirbrakeMiddleware>(config);
        }

        /// <summary>
        /// Adds the Airbrake middleware to the request pipeline.
        /// Uses provided the <see cref="AirbrakeNotifier"/> instance.
        /// </summary>
        public static IApplicationBuilder UseAirbrake(this IApplicationBuilder app, AirbrakeNotifier notifier)
        {
            return app.UseMiddleware<AirbrakeMiddleware>(notifier);
        }
    }
}
