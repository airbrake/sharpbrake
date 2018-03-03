using System;
using System.Configuration;
using System.Web;
using System.Linq;
using Sharpbrake.Client;
using Sharpbrake.Web;

namespace Sharpbrake.Http.Module
{
    public class AirbrakeHttpModule : IHttpModule
    {
        private readonly AirbrakeNotifier notifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeHttpModule"/> class.
        /// </summary>
        public AirbrakeHttpModule()
        {
            var settings = ConfigurationManager.AppSettings.AllKeys
                .Where(key => key.StartsWith("Airbrake", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

            var config = AirbrakeConfig.Load(settings);

            notifier = new AirbrakeNotifier(config);
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <remarks>
        /// The <see cref="HttpApplication.Error"/> occurs when an unhandled exception is thrown.
        /// AirbrakeHttpModule subscribes to that event and notifies Airbrake on exception.
        /// </remarks>
        public void Init(HttpApplication application)
        {
            application.Error += (sender, args) =>
            {
                var app = (HttpApplication) sender;
                var exception = app.Server.GetLastError().GetBaseException();
                var context = new AspNetHttpContext(app.Context);

                var notice = notifier.CreateNotice(exception);
                notifier.SetHttpContext(notice, context);
                notifier.NotifyAsync(notice);
            };
        }

        /// <summary>
        /// Gets the <see cref="AirbrakeNotifier"/> that the module was initialized with.
        /// </summary>
        public AirbrakeNotifier GetNotifier()
        {
            return notifier;
        }

        /// <summary>
        /// Disposes the unmanaged (other than memory) resources used by the module.
        /// </summary>
        /// <remarks>
        /// AirbrakeHttpModule doesn't demand on resources that should be disposed explicitly.
        /// </remarks>
        public void Dispose() { }
    }
}
