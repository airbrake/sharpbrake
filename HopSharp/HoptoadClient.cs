using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using HopSharp.Serialization;

namespace HopSharp
{
	public class HoptoadClient
	{
		public void Send(Exception e)
		{
			var notice = new HoptoadNotice();

			var error = new HoptoadError {
				Class = e.GetType().FullName,
				Message = e.GetType().Name + ": " + e.Message,
				Backtrace = new[] { new TraceLine() { File = "unknown.cs", LineNumber = 0, Method = "unknown" } }
			};

			notice.Error = error;

			//TODO: set up request, session and server headers

			// Send the notice
			this.Send(notice);
		}

		public void Send(HoptoadNotice notice)
		{
			try
			{
				// If no API key, get it from the appSettings
				if (string.IsNullOrEmpty(notice.ApiKey))
				{
					// If none is set, just return... throwing an exception is pointless, since one was already thrown!
					if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["Hoptoad:ApiKey"]))
						return;
					notice.ApiKey = ConfigurationManager.AppSettings["Hoptoad:ApiKey"];
				}

				// Create the web request
				HttpWebRequest request = WebRequest.Create("http://hoptoadapp.com/notices/") as HttpWebRequest;
				if (request == null)
					return;

				// Set the basic headers
				request.ContentType = "application/json";
				request.Accept = "text/xml, application/xml";
				request.KeepAlive = false;

				// It is important to set the method late... .NET quirk, it will interfere with headers set after
				request.Method = "POST";

				// Go populate the body
				SetRequestBody(request, notice);

				// Begin the request, yay async
				request.BeginGetResponse(RequestCallback, null);
			}
			catch
			{
				// Since an exception was already thrown, allowing another one to bubble up is pointless
				// But we should log it or something
				// TODO this could be better
			}
		}

		private void RequestCallback(IAsyncResult ar)
		{
			// Get it back
			var request = ar.AsyncState as HttpWebRequest;
			if (request == null)
				return;

			// We want to swallow any error responses
			try
			{
				request.EndGetResponse(ar);
			}
			catch (WebException e)
			{
				// Since an exception was already thrown, allowing another one to bubble up is pointless
				// But we should log it or something
				// TODO this could be better
				Console.WriteLine("." + e.Message + ".");
				var sr = new StreamReader(e.Response.GetResponseStream());
				Console.WriteLine(sr.ReadToEnd());
				sr.Close();
			}
		}

		private void SetRequestBody(HttpWebRequest request, HoptoadNotice notice)
		{
			var serializer = new CleanXmlSerializer<HoptoadNotice>();
			var xml = serializer.ToXml(notice);

			var payload = Encoding.UTF8.GetBytes(xml);
			request.ContentLength = payload.Length;

			using (var stream = request.GetRequestStream())
			{
				stream.Write(payload, 0, payload.Length);
				stream.Close();
			}
		}
	}
}