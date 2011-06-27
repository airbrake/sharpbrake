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
   /// The client responsible for communicating exceptions to the Hoptoad service.
   /// </summary>
    public class HoptoadClient
    {
        const string hoptoadUri = "http://hoptoadapp.com/notifier_api/v2/notices";
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
        /// Occurs when the request ends.
        /// </summary>
        public event RequestEndEventHandler RequestEnd;


        /// <summary>
        /// Sends the specified exception to Hoptoad.
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
        /// Sends the specified notice to Hoptoad.
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
                    if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["Hoptoad:ApiKey"]))
                    {
                        _log.Fatal("No 'Hoptoad:ApiKey' found. Please define one in AppSettings.");
                        return;
                    }

                   notice.ApiKey = _builder.Configuration.ApiKey;
                }

                // Create the web request
                var request = WebRequest.Create(hoptoadUri) as HttpWebRequest;

                if (request == null)
                {
                    _log.FatalFormat("Couldn't create a request to '{0}'.", hoptoadUri);
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
                _log.Fatal("An error occurred while trying to send to Hoptoad.", exception);
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
                 _log.DebugFormat("Received from Hoptoad:\n{0}", responseBody);
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
           _log.DebugFormat("{0}.RequestCallback({1})", GetType(), ar);
           
            // Get it back
            var request = ar.AsyncState as HttpWebRequest;

            if (request == null)
            {
                _log.FatalFormat("{0}.AsyncState was null or not of type {1}.", typeof(IAsyncResult), typeof(HttpWebRequest));
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
                _log.Fatal("An error occurred while retrieving the web response", exception);
                response = exception.Response;
            }

            OnRequestEnd(request, response);
        }

        private void SetRequestBody(WebRequest request, HoptoadNotice notice)
        {
            var serializer = new CleanXmlSerializer<HoptoadNotice>();
            string xml = serializer.ToXml(notice);

            _log.DebugFormat("Sending the following to '{0}':\n{1}", request.RequestUri, xml);

            byte[] payload = Encoding.UTF8.GetBytes(xml);
            request.ContentLength = payload.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(payload, 0, payload.Length);
            }
        }
    }
}