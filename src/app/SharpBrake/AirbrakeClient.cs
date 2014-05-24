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
        private readonly AirbrakeNoticeBuilder builder;
        private readonly AirbrakeConfiguration configuration;
        private readonly ILog log;


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeClient"/> class.
        /// </summary>
        public AirbrakeClient()
            : this(new AirbrakeConfiguration(), new BacktraceBuilder(LogManager.GetLogger<BacktraceBuilder>()))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeClient"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="backtraceBuilder"> </param>
        public AirbrakeClient(AirbrakeConfiguration configuration, IBuilder<Exception, Backtrace> backtraceBuilder = null)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;

            if (backtraceBuilder == null)
                backtraceBuilder = new BacktraceBuilder(LogManager.GetLogger<BacktraceBuilder>());
            this.builder = new AirbrakeNoticeBuilder(configuration,backtraceBuilder);

            this.log = LogManager.GetLogger(GetType());
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
            AirbrakeNotice notice = this.builder.Notice(exception);

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
            this.log.Debug(f => f("{0}.Send({1})", GetType(), notice));

            try
            {
                // If no API key, get it from the appSettings
                if (String.IsNullOrEmpty(notice.ApiKey))
                {
                    // If none is set, just return... throwing an exception is pointless, since one was already thrown!
                    if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["Airbrake.ApiKey"]))
                    {
                        this.log.Fatal("No 'Airbrake.ApiKey' found. Please define one in AppSettings.");
                        return;
                    }

                    notice.ApiKey = this.builder.Configuration.ApiKey;
                }

                // Create the web request
                var request = WebRequest.Create(this.configuration.ServerUri) as HttpWebRequest;

                if (request == null)
                {
                    this.log.Fatal(f => f("Couldn't create a request to '{0}'.", this.configuration.ServerUri));
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
                this.log.Fatal("An error occurred while trying to send to Airbrake.", exception);
            }
        }


        private void OnRequestEnd(WebRequest request, WebResponse response)
        {
            if (response == null)
            {
                this.log.Fatal(f => f("No response received!"));
                return;
            }

            string responseBody;

            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                    return;

                using (var sr = new StreamReader(responseStream))
                {
                    responseBody = sr.ReadToEnd();
                    this.log.Debug(f => f("Received from Airbrake.\n{0}", responseBody));
                }
            }

            if (RequestEnd != null)
            {
                RequestEndEventArgs e = new RequestEndEventArgs(request, response, responseBody);
                RequestEnd(this, e);
            }
        }


        private void RequestCallback(IAsyncResult result)
        {
            this.log.Debug(f => f("{0}.RequestCallback({1})", GetType(), result));

            // Get it back
            var request = result.AsyncState as HttpWebRequest;

            if (request == null)
            {
                this.log.Fatal(
                    f => f(
                        "{0}.AsyncState was null or not of type {1}.",
                        typeof(IAsyncResult),
                        typeof(HttpWebRequest)));
                return;
            }

            WebResponse response;

            // We want to swallow any error responses
            try
            {
                response = request.EndGetResponse(result);
            }
            catch (WebException exception)
            {
                // Since an exception was already thrown, allowing another one to bubble up is pointless
                this.log.Fatal("An error occurred while retrieving the web response", exception);
                response = exception.Response;
            }

            OnRequestEnd(request, response);
        }


        private void SetRequestBody(WebRequest request, AirbrakeNotice notice)
        {
            var serializer = new CleanXmlSerializer<AirbrakeNotice>();
            string xml = serializer.ToXml(notice);

            this.log.Debug(f => f("Sending the following to '{0}':\n{1}", request.RequestUri, xml));

            byte[] payload = Encoding.UTF8.GetBytes(xml);
            request.ContentLength = payload.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(payload, 0, payload.Length);
            }
        }
    }
}