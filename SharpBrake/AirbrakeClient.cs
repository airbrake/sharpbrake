using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

using Common.Logging;

using SharpBrake.Serialization;

namespace SharpBrake
{
	/// <summary>
	/// The client responsible for communicating exceptions to the Airbrake service.
	/// </summary>
	public class AirbrakeClient
	{
		private const string airbrakeUri = "http://airbrakeapp.com/notifier_api/v2/notices";
		private readonly AirbrakeNoticeBuilder _builder;
		private readonly ILog _log;


		/// <summary>
		/// Initializes a new instance of the <see cref="AirbrakeClient"/> class.
		/// </summary>
		public AirbrakeClient()
		{
			this._builder = new AirbrakeNoticeBuilder();
			this._log = LogManager.GetLogger(GetType());
		}


		/// <summary>
		/// Occurs when the request ends.
		/// </summary>
		public event RequestEndEventHandler RequestEnd;


		/// <summary>
		/// Sends the specified exception to Airbrake.
		/// </summary>
		/// <param name="exception">The e.</param>
		public void Send(Exception exception)
		{
			AirbrakeNotice notice = this._builder.Notice(exception);

			//TODO: set up request, session and server headers
			// Why would that be necessary, it's set in Send(AirbrakeNotice), isn't it? - @asbjornu

			// Send the notice
			Send(notice);
		}


		/// <summary>
		/// Sends the specified notice to Airbrake.
		/// </summary>
		/// <param name="notice">The notice.</param>
		public void Send(AirbrakeNotice notice)
		{
			this._log.DebugFormat("{0}.Send({1})", GetType(), notice);

			try
			{
				// If no API key, get it from the appSettings
				if (String.IsNullOrEmpty(notice.ApiKey))
				{
					// If none is set, just return... throwing an exception is pointless, since one was already thrown!
					if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["Airbrake.ApiKey"]))
					{
						this._log.Fatal("No 'Airbrake.ApiKey' found. Please define one in AppSettings.");
						return;
					}

					notice.ApiKey = this._builder.Configuration.ApiKey;
				}

				// Create the web request
				var request = WebRequest.Create(airbrakeUri) as HttpWebRequest;

				if (request == null)
				{
					this._log.FatalFormat("Couldn't create a request to '{0}'.", airbrakeUri);
					return;
				}

				// Set the basic headers
				request.ContentType = "text/xml";
				request.Accept = "text/xml";
				request.KeepAlive = false;

				// It is important to set the method late... .NET quirk, it will interfere with headers set after
				request.Method = "POST";

				// Go populate the body
				SetRequestBody(request, notice);

				// Begin the request, yay async
				request.BeginGetResponse(RequestCallback, request);
			}
			catch (Exception exception)
			{
				this._log.Fatal("An error occurred while trying to send to Airbrake.", exception);
			}
		}


		private void OnRequestEnd(WebRequest request, WebResponse response)
		{
			string responseBody;

			using (var responseStream = response.GetResponseStream())
			{
				if (responseStream == null)
					return;

				using (var sr = new StreamReader(responseStream))
				{
					responseBody = sr.ReadToEnd();
					this._log.DebugFormat("Received from Airbrake.\n{0}", responseBody);
				}
			}

			if (RequestEnd != null)
			{
				RequestEndEventArgs e = new RequestEndEventArgs(request, response, responseBody);
				RequestEnd(this, e);
			}
		}


		private void RequestCallback(IAsyncResult ar)
		{
			this._log.DebugFormat("{0}.RequestCallback({1})", GetType(), ar);

			// Get it back
			var request = ar.AsyncState as HttpWebRequest;

			if (request == null)
			{
				this._log.FatalFormat("{0}.AsyncState was null or not of type {1}.", typeof(IAsyncResult), typeof(HttpWebRequest));
				return;
			}

			WebResponse response;

			// We want to swallow any error responses
			try
			{
				response = request.EndGetResponse(ar);
			}
			catch (WebException exception)
			{
				// Since an exception was already thrown, allowing another one to bubble up is pointless
				this._log.Fatal("An error occurred while retrieving the web response", exception);
				response = exception.Response;
			}

			OnRequestEnd(request, response);
		}


		private void SetRequestBody(WebRequest request, AirbrakeNotice notice)
		{
			var serializer = new CleanXmlSerializer<AirbrakeNotice>();
			string xml = serializer.ToXml(notice);

			this._log.DebugFormat("Sending the following to '{0}':\n{1}", request.RequestUri, xml);

			byte[] payload = Encoding.UTF8.GetBytes(xml);
			request.ContentLength = payload.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(payload, 0, payload.Length);
			}
		}
	}
}