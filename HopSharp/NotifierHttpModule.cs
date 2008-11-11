using System;
using System.Web;

namespace HopSharp
{
	public class NotifierHttpModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.Error += new EventHandler(context_Error);
		}

		void context_Error(object sender, EventArgs e)
		{
			HttpApplication application = (HttpApplication)sender;
			HoptoadClient client = new HoptoadClient();

			Exception exception = application.Server.GetLastError();
			client.Send(exception);
		}

		public void Dispose()  { }
	}
}