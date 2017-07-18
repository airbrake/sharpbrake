using System;
using NLog;
using NLog.Targets;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;

namespace Sharpbrake.NLog
{
    [Target("Airbrake")]
    public class AirbrakeTarget : TargetWithLayout
    {
        private readonly AirbrakeNotifier notifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeTarget"/> class.
        /// </summary>
        public AirbrakeTarget(AirbrakeNotifier notifier)
        {
            this.notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        /// <summary>
        /// Maps NLog log level onto <see cref="Severity"/> of the error.
        /// </summary>
        private static Severity GetErrorSeverity(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Fatal)
                return Severity.Critical;

            if (logLevel == LogLevel.Error)
                return Severity.Error;

            if (logLevel == LogLevel.Warn)
                return Severity.Warning;

            if (logLevel == LogLevel.Info)
                return Severity.Info;

            if (logLevel == LogLevel.Debug)
                return Severity.Debug;

            if (logLevel == LogLevel.Trace)
                return Severity.Notice;

            return Severity.Error;
        }

        /// <summary>
        /// Notifies Airbrake on the logging event exception.
        /// </summary>
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Exception == null)
                return;

            notifier.NotifyAsync(logEvent.Exception, GetHttpContext(), GetErrorSeverity(logEvent.Level));
        }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        protected virtual IHttpContext GetHttpContext()
        {
            // in default case (non Web) HTTP context is not available
            return null;
        }
    }
}
