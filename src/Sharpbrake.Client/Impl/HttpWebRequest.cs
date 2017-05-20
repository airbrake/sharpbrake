using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Sharpbrake.Client.Impl
{
    /// <summary>
    /// Implementation of <see cref="IHttpRequest"/> based on the <see cref="System.Net.HttpWebRequest"/> class.
    /// </summary>
    public class HttpWebRequest : IHttpRequest
    {
        private readonly System.Net.HttpWebRequest request;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebRequest"/> class.
        /// </summary>
        /// <param name="request">The underlying <see cref="System.Net.HttpWebRequest"/> that the current <see cref="HttpWebRequest"/> is based on.</param>
        public HttpWebRequest(System.Net.HttpWebRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            this.request = request;
        }

        /// <summary>
        /// Creates an instance of <see cref="System.Net.HttpWebRequest"/> with specified endpoint.
        /// </summary>
        public static HttpWebRequest Create(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException("endpoint");

            return new HttpWebRequest((System.Net.HttpWebRequest)WebRequest.Create(endpoint));
        }

        /// <summary>
        /// Gets URI of the request (endpoint).
        /// </summary>
        public Uri RequestUri
        {
            get { return request.RequestUri; }
        }

        /// <summary>
        /// Gets or sets the value of Content-Type HTTP header.
        /// </summary>
        public string ContentType
        {
            get { return request.ContentType; }
            set { request.ContentType = value; }
        }

        /// <summary>
        /// Gets or sets the value of Accept HTTP header.
        /// </summary>
        public string Accept
        {
            get { return request.Accept; }
            set { request.Accept = value; }
        }

        /// <summary>
        /// Gets or sets HTTP method for the request.
        /// </summary>
        public string Method
        {
            get { return request.Method; }
            set { request.Method = value; }
        }

        /// <summary>
        /// Gets or sets the proxy to access the network resource.
        /// </summary>
        public IWebProxy Proxy
        {
            get { return request.Proxy; }
            set { request.Proxy = value; }
        }

        /// <summary>
        /// Gets the GetRequestStreamAsync implementation of underlying HttpWebRequest class.
        /// </summary>
        public Task<Stream> GetRequestStreamAsync()
        {
            return request.GetRequestStreamAsync();
        }

        /// <summary>
        /// Gets GetResponseAsync implementation of the underlying HttpWebRequest class
        /// converted to the IHttpResponse interface.
        /// </summary>
        public Task<IHttpResponse> GetResponseAsync()
        {
            var tcs = new TaskCompletionSource<IHttpResponse>();
            request.GetResponseAsync().ContinueWith(responseTask =>
            {
                if (responseTask.IsFaulted)
                {
                    if (responseTask.Exception != null)
                        tcs.SetException(responseTask.Exception.InnerExceptions);
                }
                else if (responseTask.IsCanceled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    var response = responseTask.Result;
                    tcs.SetResult(new HttpWebResponse((System.Net.HttpWebResponse)response));
                }
            });
            return tcs.Task;
        }
    }
}
