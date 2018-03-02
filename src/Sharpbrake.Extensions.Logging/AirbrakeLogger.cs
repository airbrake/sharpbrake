using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;
using Sharpbrake.Web;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sharpbrake.Extensions.Logging
{
    /// <summary>
    /// Logger that sends an error to the Airbrake dashboard.
    /// </summary>
    public class AirbrakeLogger : ILogger
    {
        private readonly AirbrakeNotifier notifier;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly LogLevel minLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeLogger"/> class.
        /// </summary>
        public AirbrakeLogger(AirbrakeNotifier notifier, IHttpContextAccessor contextAccessor, LogLevel minLevel)
        {
            this.notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            this.contextAccessor = contextAccessor;
            this.minLevel = minLevel;
        }

        /// <summary>
        /// Notifies the Airbrake endpoint on an error.
        /// </summary>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = string.Empty;
            if (formatter != null)
                message = formatter(state, exception);

            notifier.ForContext(GetHttpContext()).NotifyAsync(GetErrorSeverity(logLevel), exception, message);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= minLevel;
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            // TODO: Consider what scopes may look like in our case... One option: a bunch of exceptions
            // that are sent to the dashboard in a single call.
            return NoopDisposable.Instance;
        }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        private IHttpContext GetHttpContext()
        {
            var httpContext = contextAccessor?.HttpContext;
            return httpContext == null ? null : new AspNetCoreHttpContext(httpContext);
        }

        /// <summary>
        /// Maps <see cref="LogLevel"/> onto <see cref="Severity"/> of the error.
        /// </summary>
        private static Severity GetErrorSeverity(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Critical)
                return Severity.Critical;

            if (logLevel == LogLevel.Error)
                return Severity.Error;

            if (logLevel == LogLevel.Warning)
                return Severity.Warning;

            if (logLevel == LogLevel.Information)
                return Severity.Info;

            if (logLevel == LogLevel.Debug)
                return Severity.Debug;

            if (logLevel == LogLevel.Trace)
                return Severity.Notice;

            return Severity.Error;
        }

        /// <summary>
        /// Fake class to satisfy the IDisposable interface.
        /// </summary>
        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
