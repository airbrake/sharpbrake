using log4net.Appender;
using log4net.Core;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;

namespace Sharpbrake.Log4net
{
    /// <summary>
    /// Appender that sends an exception from logging request to Airbrake.
    /// </summary>
    public class AirbrakeAppender : AppenderSkeleton
    {
        /// <summary>
        /// Instance of <see cref="AirbrakeNotifier"/> that is used to communicate with Airbrake.
        /// </summary>
        public AirbrakeNotifier Notifier { get; private set; }

        /// <summary>
        /// Name of your environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Version of the application that uses the notifier.
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// User API key is used to access to the project
        /// data through Airbrake APIs. Each user of a project has their own key.
        /// </summary>
        public string ProjectKey { get; set; }

        /// <summary>
        /// Project API key that is used to submit errors and track deploys. 
        /// This key is what you configure the notifier agent in your app to use.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Host name of the endpoint.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Path to logging file.
        /// </summary>
        public string LogFile { get; set; }

        /// <summary>
        /// Represents URI string of proxy provider.
        /// </summary>
        public string ProxyUri { get; set; }

        /// <summary>
        /// The credentials for proxy.
        /// </summary>
        public string ProxyUsername { get; set; }

        /// <summary>
        /// The credentials for proxy.
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// Comma-separated list of environments that should be ignored.
        /// </summary>
        public string IgnoreEnvironments { get; set; }

        /// <summary>
        /// Comma-separated list of parameters which will not be filtered.
        /// If count of parameters is not zero - all not listed parameters will be filtered.
        /// </summary>
        public string WhitelistKeys { get; set; }

        /// <summary>
        /// Comma-separated list of parameters which will be filtered out.
        /// </summary>
        public string BlacklistKeys { get; set; }

        /// <summary>
        /// Initializes the appender based on the options set.
        /// </summary>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            var config = new AirbrakeConfig
            {
                Environment = Environment,
                AppVersion = AppVersion,
                ProjectKey = ProjectKey,
                ProjectId = ProjectId,
                Host = Host,
                LogFile = LogFile,
                ProxyUri = ProxyUri,
                ProxyUsername = ProxyUsername,
                ProxyPassword = ProxyPassword,
                IgnoreEnvironments = Utils.ParseParameter(IgnoreEnvironments),
                WhitelistKeys = Utils.ParseParameter(WhitelistKeys),
                BlacklistKeys = Utils.ParseParameter(BlacklistKeys)
            };

            Notifier = new AirbrakeNotifier(config);
        }

        /// <summary>
        /// Maps log4net log level onto <see cref="Severity"/> of the error.
        /// </summary>
        private static Severity GetErrorSeverity(Level logLevel)
        {
            switch (logLevel.Name)
            {
                case "DEBUG":
                    return Severity.Debug;
                case "WARN":
                    return Severity.Warning;
                case "INFO":
                    return Severity.Info;
                case "FATAL":
                    return Severity.Critical;
                default:
                    return Severity.Error;
            }
        }

        /// <summary>
        /// Writes out an exception from logging event.
        /// </summary>
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent?.ExceptionObject == null)
                return;

            Notifier.NotifyAsync(loggingEvent.ExceptionObject, GetHttpContext(), GetErrorSeverity(loggingEvent.Level));
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
