#if NETSTANDARD1_4 || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Sharpbrake.Client;

namespace Sharpbrake.Web
{
    /// <summary>
    /// Implementation of <see cref="IHttpContext"/> based on ASP.NET Core HttpContext.
    /// </summary>
    public class AspNetCoreHttpContext : IHttpContext
    {
        public IDictionary<string, string> Session { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public IDictionary<string, string> EnvironmentVars { get; set; }

        public string UserAddr { get; set; }
        public string UserAgent { get; set; }
        public string Url { get; set; }

        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }

        public string Action { get; set; }
        public string Component { get; set; }

        public AspNetCoreHttpContext(HttpContext context)
        {
            if (TryGet(() => context.Session) != null && context.Session.Keys.Any())
            {
                var session = new Dictionary<string, string>();
                foreach (var key in context.Session.Keys)
                {
                    byte[] sessionData;
                    context.Session.TryGetValue(key, out sessionData);
                    if (sessionData != null)
                    {
                        session.Add(key, Encoding.UTF8.GetString(sessionData));
                    }
                }
                Session = session;
            }

            if (TryGet(() => context.Request.Form) != null && context.Request.Form.Keys.Count != 0)
            {
                var parameters = new Dictionary<string, string>();

                foreach (var key in context.Request.Form.Keys)
                {
                    StringValues formData;
                    context.Request.Form.TryGetValue(key, out formData);
                    parameters.Add(key, formData.ToString());
                }

                Parameters = parameters;
            }

            if (TryGet(() => context.Request.Headers) != null && context.Request.Headers.Count != 0)
            {
                var environmentVars = context.Request.Headers.Keys.ToDictionary<string, string, string>(key => key, key => context.Request.Headers[key]);
                EnvironmentVars = environmentVars;

                UserAddr = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                UserAgent = context.Request.Headers["User-Agent"].ToString();
            }

            UserName = context.User.Identity.Name;
            Url = context.Request.Path.ToUriComponent();

            var routingFeature = context.Features[typeof(IRoutingFeature)] as IRoutingFeature;
            if (routingFeature != null)
            {
                var routeData = routingFeature.RouteData;

                if (routeData.Values.ContainsKey("action"))
                    Action = routeData.Values["action"].ToString();

                if (routeData.Values.ContainsKey("controller"))
                    Component = routeData.Values["controller"].ToString();
            }
        }

        private static T TryGet<T>(Func<T> getter) where T : class
        {
            try
            {
                return getter();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
#endif
