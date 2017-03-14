using System.IO;
using System.Net;

namespace Sharpbrake.Client
{
    /// <summary>
    /// An interface that represents HTTP response functionality.
    /// </summary>
    public interface IHttpResponse
    {
        /// <summary>
        /// Gets the status of HTTP response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets <see cref="Stream"/> for reading content from the server.
        /// </summary>
        Stream GetResponseStream();
    }
}
