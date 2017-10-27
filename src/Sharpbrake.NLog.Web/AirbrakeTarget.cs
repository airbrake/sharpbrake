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
        /// Gets the current HTTP context.
        /// </summary>
        protected override IHttpContext GetHttpContext()
        {
            var httpContext = System.Web.HttpContext.Current;
            return httpContext == null ? null : new AspNetHttpContext(httpContext);
        }
#elif NETSTANDARD1_4 || NETSTANDARD2_0
        public Microsoft.AspNetCore.Http.IHttpContextAccessor ContextAccessor { get; set; }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        protected override IHttpContext GetHttpContext()
        {
            var httpContext = ContextAccessor?.HttpContext;
            return httpContext == null ? null : new AspNetCoreHttpContext(httpContext);
        }
#endif
    }
}
