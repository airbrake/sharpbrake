using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

using Common.Logging;

using HopSharp.Serialization;

namespace HopSharp
{
   /// <summary>
   /// The client responsible for communicating exceptions to the HopToad service.
   /// </summary>
    public class HoptoadClient
    {
        private readonly HoptoadNoticeBuilder _builder;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoptoadClient"/> class.
        /// </summary>
        public HoptoadClient()
        {
            _builder = new HoptoadNoticeBuilder();
            _log = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Sends the specified exception to HopToad.
        /// </summary>
        /// <param name="exception">The e.</param>
        public void Send(Exception exception)
        {
            HoptoadNotice notice = _builder.Notice(exception);

            //TODO: set up request, session and server headers
            // Why would that be necessary, it's set in Send(HoptoadNotice), isn't it? - @asbjornu

            // Send the notice
            Send(notice);
        }

        /// <summary>
        /// Sends the specified notice to HopToad.
        /// </summary>
        /// <param name="notice">The notice.</param>
        public void Send(HoptoadNotice notice)
        {
            _log.DebugFormat("{0}.Send({1})", GetType(), notice);

            try
            {
                // If no API key, get it from the appSettings
                if (String.IsNullOrEmpty(notice.ApiKey))
                {
                    // If none is set, just return... throwing an exception is pointless, since one was already thrown!
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["Hoptoad:ApiKey"]))
                        return;

                    notice.ApiKey = _builder.Configuration.ApiKey;
                }

                // Create the web request
                var request = WebRequest.Create("http://hoptoadapp.com/notifier_api/v2/notices") as HttpWebRequest;
                
                if (request == null)
                    return;

                // Set the basic headers
                request.ContentType = "text/xml";
                request.Accept = "text/xml";
                request.KeepAlive = false;

                // It is important to set the method late... .NET quirk, it will interfere with headers set after
                request.Method = "POST";

                // Go populate the body
                SetRequestBody(request, notice);

                // Begin the request, yay async
                request.BeginGetResponse(RequestCallback, null);
            }
            catch (Exception exception)
            {
                _log.Fatal("An error occurred while trying to send to HopToad.", exception);
            }
        }

        private void RequestCallback(IAsyncResult ar)
        {
           _log.DebugFormat("{0}.RequestCallback({1})", GetType(), ar);
           
            // Get it back
            var request = ar.AsyncState as HttpWebRequest;

            if (request == null)
            {
                _log.FatalFormat("{0}.AsyncState was null or not of type {1}.", ar.AsyncState, typeof(HttpWebRequest));
                return;
            }

            // We want to swallow any error responses
            try
            {
                request.EndGetResponse(ar);
            }
            catch (WebException exception)
            {
                // Since an exception was already thrown, allowing another one to bubble up is pointless
                // But we should log it or something
                _log.Fatal("An error occurred while retrieving the web response", exception);

                using (var responseStream = exception.Response.GetResponseStream())
                {
                   if (responseStream == null)
                       return;

                   using (var sr = new StreamReader(responseStream))
                   {
                      _log.Debug(sr.ReadToEnd());
                   }
                }
            }
        }

        private static void SetRequestBody(WebRequest request, HoptoadNotice notice)
        {
            var serializer = new CleanXmlSerializer<HoptoadNotice>();
            string xml = serializer.ToXml(notice);

            byte[] payload = Encoding.UTF8.GetBytes(xml);
            request.ContentLength = payload.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(payload, 0, payload.Length);
            }
        }
    }
}