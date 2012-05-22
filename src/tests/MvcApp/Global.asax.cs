using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SharpBrake.MvcApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }


        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
            routes.MapRoute("Default", "{Controller}/{Action}", new { Controller = "Home", Action = "Index" });
        }


        private void Application_Error(object sender, EventArgs e)
        {
            Exception lastError = Server.GetLastError();
            lastError.SendToAirbrake();
            Server.ClearError();
        }
    }
}