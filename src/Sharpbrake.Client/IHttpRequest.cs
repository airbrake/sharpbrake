using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

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

        /// <summary>
        /// Gets a <see cref="Stream"/> for writing request data to the Internet resource as an asynchronous operation.
        /// </summary>
        Task<Stream> GetRequestStreamAsync();

        /// <summary>
        /// Gets response for request as an asynchronous operation.
        /// </summary>
        Task<IHttpResponse> GetResponseAsync();
    }
}
