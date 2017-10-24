#if NETSTANDARD2_0
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharpbrake.Client;

namespace Sharpbrake.Extensions.Logging
{
    /// <summary>
    /// Extension methods for the <see cref="ILoggingBuilder"/> class to add the Airbrake logger.
    /// </summary>
    public static class AirbrakeLoggingBuilderExtensions
    {
        /// <summary>
        /// Adds the Airbrake logger to <see cref="ILoggingBuilder"/>.
        /// Logger is built using provided <see cref="AirbrakeNotifier"/> instance.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <param name="notifier">The <see cref="AirbrakeNotifier"/> instance.</param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to access the current HTTP context.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged.</param>
        public static ILoggingBuilder AddAirbrake(this ILoggingBuilder builder, AirbrakeNotifier notifier,
            IHttpContextAccessor contextAccessor = null,
            LogLevel minLevel = LogLevel.Error)
        {
            if (notifier == null)
                throw new ArgumentNullException(nameof(notifier));

            builder.AddProvider(new AirbrakeLoggerProvider(notifier, contextAccessor, minLevel));
            return builder;
        }

        /// <summary>
        /// Adds the Airbrake logger to <see cref="ILoggingBuilder"/>.
        /// Logger is built using provided <see cref="AirbrakeConfig"/> configuration.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <param name="config">The <see cref="AirbrakeConfig"/> with options for the Airbrake notifier.</param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to access the current HTTP context.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged.</param>
        public static ILoggingBuilder AddAirbrake(this ILoggingBuilder builder, AirbrakeConfig config,
            IHttpContextAccessor contextAccessor = null,
            LogLevel minLevel = LogLevel.Error)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            builder.AddProvider(new AirbrakeLoggerProvider(config, contextAccessor, minLevel));
            return builder;
        }

        /// <summary>
        /// Adds the Airbrake logger to <see cref="ILoggingBuilder"/>.
        /// Logger is built using <see cref="IConfiguration"/> section with options for <see cref="AirbrakeNotifier"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/> with options for the Airbrake notifier.
        /// <example><code>Configuration.GetSection("Airbrake")</code></example>
        /// </param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to access the current HTTP context.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged.</param>
        public static ILoggingBuilder AddAirbrake(this ILoggingBuilder builder, IConfiguration config,
            IHttpContextAccessor contextAccessor = null,
            LogLevel minLevel = LogLevel.Error)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var settings = config.GetChildren()
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            var airbrakeConfig = AirbrakeConfig.Load(settings);

            builder.AddProvider(new AirbrakeLoggerProvider(airbrakeConfig, contextAccessor, minLevel));
            return builder;
        }
    }
}
#endif
