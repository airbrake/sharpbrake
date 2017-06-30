using System;
using System.IO;
using System.Net;

namespace Sharpbrake.Client.Impl
{
    /// <summary>
    /// Implementation of <see cref="IHttpResponse"/> based on the <see cref="System.Net.HttpWebResponse"/> class.
    /// </summary>
    public class HttpWebResponse : IHttpResponse, IDisposable
    {
        private System.Net.HttpWebResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebResponse"/> class.
        /// </summary>
        public HttpWebResponse(System.Net.HttpWebResponse response)
        {
            this.response = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// Gets the status of HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode => response.StatusCode;

        /// <summary>
        /// Gets the <see cref="Stream"/> for reading content from the server.
        /// </summary>
        public Stream GetResponseStream()
        {
            return response.GetResponseStream();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable) response)?.Dispose();
            }
        }
    }
}
