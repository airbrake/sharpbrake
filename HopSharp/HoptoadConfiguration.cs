using System;
using System.Configuration;
using System.Web;

namespace HopSharp
{
    public class HoptoadConfiguration
    {
        public HoptoadConfiguration()
        {
            ApiKey = ConfigurationManager.AppSettings["Hoptoad:ApiKey"];
            EnvironmentName = ConfigurationManager.AppSettings["Hoptoad:Environment"];

            ProjectRoot = HttpContext.Current != null ? HttpContext.Current.Request.ApplicationPath : Environment.CurrentDirectory;
        }


        public string ProjectRoot { get; set; }
        public string ApiKey { get; set; }

        public string EnvironmentName { get; set; }
    }
}