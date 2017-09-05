using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharpbrake.Client;

namespace Sharpbrake.Extensions.Logging
{
    /// <summary>
    /// Extension methods for the <see cref="ILoggerFactory"/> class to add the Airbrake logger.
    /// </summary>
    public static class AirbrakeLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds the Airbrake logger to the factory.
        /// Logger is built using provided <see cref="AirbrakeNotifier"/> instance.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="notifier">The <see cref="AirbrakeNotifier"/> instance.</param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to access the current HTTP context.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged.</param>
        public static ILoggerFactory AddAirbrake(this ILoggerFactory factory, AirbrakeNotifier notifier,
            IHttpContextAccessor contextAccessor = null,
            LogLevel minLevel = LogLevel.Error)
        {
            if (notifier == null)
                throw new ArgumentNullException(nameof(notifier));

            factory.AddProvider(new AirbrakeLoggerProvider(notifier, contextAccessor, minLevel));
            return factory;
        }

        /// <summary>
        /// Adds the Airbrake logger to the factory.
        /// Logger is built using provided <see cref="AirbrakeConfig"/> configuration.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="config">The <see cref="AirbrakeConfig"/> with options for the Airbrake notifier.</param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to access the current HTTP context.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged.</param>
        public static ILoggerFactory AddAirbrake(this ILoggerFactory factory, AirbrakeConfig config,
            IHttpContextAccessor contextAccessor = null,
            LogLevel minLevel = LogLevel.Error)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            factory.AddProvider(new AirbrakeLoggerProvider(config, contextAccessor, minLevel));
            return factory;
        }

        /// <summary>
        /// Adds the Airbrake logger to the factory.
        /// Logger is built using <see cref="IConfiguration"/> section with options for <see cref="AirbrakeNotifier"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/> with options for the Airbrake notifier.
        /// <example><code>Configuration.GetSection("Airbrake")</code></example>
        /// </param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to access the current HTTP context.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged.</param>
        public static ILoggerFactory AddAirbrake(this ILoggerFactory factory, IConfiguration config,
            IHttpContextAccessor contextAccessor = null,
            LogLevel minLevel = LogLevel.Error)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var settings = config.GetChildren()
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            var airbrakeConfig = AirbrakeConfig.Load(settings);

            factory.AddProvider(new AirbrakeLoggerProvider(airbrakeConfig, contextAccessor, minLevel));
            return factory;
        }
    }
}
