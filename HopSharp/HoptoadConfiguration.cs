using System;
using System.Configuration;
using System.Web;

namespace HopSharp
{
	public class HoptoadConfiguration
	{
		public HoptoadConfiguration()
		{
			this.ApiKey = ConfigurationManager.AppSettings["Hoptoad:ApiKey"];
			this.EnvironmentName = ConfigurationManager.AppSettings["Hoptoad:Environment"];

			if (HttpContext.Current != null)
			{
				this.ProjectRoot = HttpContext.Current.Request.ApplicationPath;
			}
			else
			{
				this.ProjectRoot = Environment.CurrentDirectory;
			}
		}


		public string ProjectRoot { get; set; }
		public string ApiKey { get; set; }

		public string EnvironmentName { get; set; }
	}
}