using System;
using System.IO;
using System.Net;
#if NET35
#else
using System.Threading.Tasks;
#endif

namespace Sharpbrake.Client
{
    /// <summary>
    /// An interface that represents HTTP request functionality.
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets URI of the request.
        /// </summary>
        Uri RequestUri { get; }

        /// <summary>
        /// Gets or sets the value of Content-Type HTTP header.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the value of Accept HTTP header.
        /// </summary>
        string Accept { get; set; }

        /// <summary>
        /// Gets or sets HTTP method for the request.
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// Gets or sets the proxy to access the network resource.
        /// </summary>
        IWebProxy Proxy { get; set; }

#if NET35
        /// <summary>
        /// Gets BeginGetRequestStream implementation.
        /// </summary>
        /// <remarks>
        /// BeginGetRequestStream method starts an asynchronous request for a stream used to send data for web request.
        /// The asynchronous callback method uses the EndGetRequestStream method to return the actual stream.
        /// </remarks>
        IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state);

        /// <summary>
        /// Gets EndGetRequestStream implementation.
        /// </summary>
        /// <remarks>
        /// EndGetRequestStream method completes an asynchronous request for a stream
        /// that was started by the BeginGetRequestStream method.
        /// </remarks>
        Stream EndGetRequestStream(IAsyncResult asyncResult);

        /// <summary>
        /// Gets BeginGetResponse implementation.
        /// </summary>
        /// <remarks>
        /// BeginGetResponse method starts an asynchronous request for a response from the Internet resource.
        /// The asynchronous callback method uses the EndGetResponse method to return the actual response.
        /// </remarks>
        IAsyncResult BeginGetResponse(AsyncCallback callback, object state);

        /// <summary>
        /// Gets EndGetResponse implementation.
        /// </summary>
        /// <remarks>
        /// EndGetResponse method completes an asynchronous request for an Internet resource
        /// that was started by calling the BeginGetResponse method.
        /// </remarks>
        IHttpResponse EndGetResponse(IAsyncResult asyncResult);
#else
        /// <summary>
        /// Gets a <see cref="Stream"/> for writing request data to the Internet resource as an asynchronous operation.
        /// </summary>
        Task<Stream> GetRequestStreamAsync();

        /// <summary>
        /// Gets response for request as an asynchronous operation.
        /// </summary>
        Task<IHttpResponse> GetResponseAsync();
#endif
    }
}
