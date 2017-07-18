using NLog.Targets;
using Sharpbrake.Client;
using Sharpbrake.Web;

namespace Sharpbrake.NLog.Web
{
    [Target("Airbrake")]
    public class AirbrakeTarget : NLog.AirbrakeTarget
    {
#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeTarget"/> class.
        /// </summary>
        public AirbrakeTarget(AirbrakeNotifier notifier) : base(notifier) {}

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        protected override IHttpContext GetHttpContext()
        {
            var httpContext = System.Web.HttpContext.Current;
            return httpContext == null ? null : new AspNetHttpContext(httpContext);
        }
#elif NETSTANDARD1_4
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeTarget"/> class.
        /// </summary>
        public AirbrakeTarget(AirbrakeNotifier notifier, Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor) : base(notifier)
        {
            this.contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        protected override IHttpContext GetHttpContext()
        {
            var httpContext = contextAccessor?.HttpContext;
            return httpContext == null ? null : new AspNetCoreHttpContext(httpContext);
        }
#endif
    }
}
