using System;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;

namespace Sharpbrake.NLog
{
    [Target("Airbrake")]
    public class AirbrakeTarget : TargetWithLayout
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
        [RequiredParameter]
        public string ProjectKey { get; set; }

        /// <summary>
        /// Project API key that is used to submit errors and track deploys. 
        /// This key is what you configure the notifier agent in your app to use.
        /// </summary>
        [RequiredParameter]
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

        public AirbrakeTarget()
        {
            Layout = "${message}";
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
        /// Initializes the target. Notifier is constructed using properties
        /// from NLog configuration file.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var config = new AirbrakeConfig
            {
                Environment = RenderSimpleLayout(Environment, nameof(Environment)),
                AppVersion = RenderSimpleLayout(AppVersion, nameof(AppVersion)),
                ProjectKey = RenderSimpleLayout(ProjectKey, nameof(ProjectKey)),
                ProjectId = RenderSimpleLayout(ProjectId, nameof(ProjectId)),
                Host = RenderSimpleLayout(Host, nameof(Host)),
                LogFile = RenderSimpleLayout(LogFile, nameof(LogFile)),
                ProxyUri = RenderSimpleLayout(ProxyUri, nameof(ProxyUri)),
                ProxyUsername = RenderSimpleLayout(ProxyUsername, nameof(ProxyUsername)),
                ProxyPassword = RenderSimpleLayout(ProxyPassword, nameof(ProxyPassword)),
                IgnoreEnvironments = Utils.ParseParameter(RenderSimpleLayout(IgnoreEnvironments, nameof(IgnoreEnvironments))),
                WhitelistKeys = Utils.ParseParameter(RenderSimpleLayout(WhitelistKeys, nameof(WhitelistKeys))),
                BlacklistKeys = Utils.ParseParameter(RenderSimpleLayout(BlacklistKeys, nameof(BlacklistKeys))),
            };

            Notifier = new AirbrakeNotifier(config);
        }

        private string RenderSimpleLayout(string simpleLayout, string propertyName)
        {
            try
            {
                return string.IsNullOrEmpty(simpleLayout) ? string.Empty : new SimpleLayout(simpleLayout).Render(LogEventInfo.CreateNullEvent());
            }
            catch
            {
                return simpleLayout;
            }
        }


        /// <summary>
        /// Notifies Airbrake on the error.
        /// </summary>
        protected override void Write(LogEventInfo logEvent)
        {
            var mesage = Layout?.Render(logEvent) ?? logEvent.FormattedMessage;
            var notice = Notifier.BuildNotice(GetErrorSeverity(logEvent.Level), logEvent.Exception, mesage);
            Notifier.SetHttpContext(notice, GetHttpContext());
            Notifier.NotifyAsync(notice);
        }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        protected virtual IHttpContext GetHttpContext()
        {
            // in the default case (non Web) the HTTP context is not available
            return null;
        }
    }
}
