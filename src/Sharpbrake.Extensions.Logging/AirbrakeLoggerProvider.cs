using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sharpbrake.Client;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sharpbrake.Extensions.Logging
{
    /// <summary>
    /// Creates instances of the <see cref="AirbrakeLogger"/> class.
    /// </summary>
    public class AirbrakeLoggerProvider : ILoggerProvider
    {
        private readonly AirbrakeNotifier notifier;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly LogLevel minLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeLoggerProvider"/> class.
        /// </summary>
        public AirbrakeLoggerProvider(AirbrakeNotifier notifier, IHttpContextAccessor contextAccessor, LogLevel minLevel)
        {
            this.notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            this.contextAccessor = contextAccessor;
            this.minLevel = minLevel;
        }

        /// <inheritdoc />
        public AirbrakeLoggerProvider(AirbrakeConfig config, IHttpContextAccessor contextAccessor, LogLevel minLevel)
            : this(new AirbrakeNotifier(config), contextAccessor, minLevel)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AirbrakeLogger"/> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        public ILogger CreateLogger(string categoryName)
        {
            return new AirbrakeLogger(notifier, contextAccessor, minLevel);
        }

        public void Dispose()
        {
        }
    }
}
