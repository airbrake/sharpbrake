using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace HopSharp
{
	public class HoptoadClient
	{
		public void Send(Exception e)
		{
			HoptoadNotice notice = new HoptoadNotice();
			notice.ApiKey = "12345678";

			// Set the exception related fields
			notice.ErrorClass = e.GetType().FullName;
			notice.ErrorMessage = e.GetType().Name + ": " + e.Message;
			notice.Backtrace = e.StackTrace;

			// If we have an HttpContext, load the environment, request, and session information
			HttpContext context = HttpContext.Current;
			if (context != null)
			{
				notice.Environment = context.Request.ServerVariables;
				notice.Request.Add("params", context.Request.Form);
				notice.Session = context.Session;
			}

			// Send the notice
			Send(notice);
		}

		public void Send(HoptoadNotice notice)
		{
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

		private void RequestCallback(IAsyncResult ar)
		{
			// Get it back
			HttpWebRequest request = ar.AsyncState as HttpWebRequest;
			if (request == null)
				return;

			// We want to swallow any error responses
			try
			{
				request.EndGetResponse(ar);
			}
			catch(WebException e)
			{
				Console.WriteLine("." + e.Message +".");
				StreamReader sr = new StreamReader(e.Response.GetResponseStream());
				Console.WriteLine(sr.ReadToEnd());
				sr.Close();
			}
		}

		private void SetRequestBody(HttpWebRequest request, HoptoadNotice notice)
		{
			// Get the bytes
			byte[] payload = Encoding.UTF8.GetBytes(notice.Serialize());
			request.ContentLength = payload.Length;

			// Get the request stream and write the bytes
			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(payload, 0, payload.Length);
				stream.Close();
			}
		}
	}
}