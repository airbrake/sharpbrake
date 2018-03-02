using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Sharpbrake.Client;
using Sharpbrake.Web;

namespace Sharpbrake.Http.Middleware
{
    /// <summary>
    /// Provides support for notifying on unhandled exceptions
    /// in the request pipeline to the Airbrake dashboard.
    /// </summary>
    public class AirbrakeMiddleware
    {
        private readonly AirbrakeNotifier notifier;
        private readonly RequestDelegate nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeMiddleware"/> class.
        /// </summary>
        public AirbrakeMiddleware(RequestDelegate next, IConfiguration config)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var settings = config.GetChildren()
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            var airbrakeConfig = AirbrakeConfig.Load(settings);

            notifier = new AirbrakeNotifier(airbrakeConfig);
            nextHandler = next;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeMiddleware"/> class.
        /// </summary>
        public AirbrakeMiddleware(RequestDelegate next, AirbrakeNotifier notifier)
        {
            this.notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            nextHandler = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Notifies on an unhandled exception that occurs downstream in the request pipeline.
        /// </summary>
        /// <remarks>
        /// Call to the next handler is wrapped in the "try-catch" block, therefore,
        /// any unhandled exception that occurs downstream in the request pipeline
        /// is caught and reported to the Airbrake dashboard.
        /// Exception is rethrown to allow components upstream in the pipeline to deal with it.
        /// An underlying notifier is exposed via an instance of the <see cref="AirbrakeFeature"/> class.
        /// Use <code>HttpContext.Features</code> collection to access it.
        /// </remarks>
        public async Task Invoke(HttpContext context)
        {
            context.Features.Set<IAirbrakeFeature>(new AirbrakeFeature(notifier));
            try
            {
                await nextHandler(context);
            }
            catch (Exception ex)
            {
                await notifier.ForContext(new AspNetCoreHttpContext(context)).NotifyAsync(ex);
                throw;
            }
        }
    }
}
