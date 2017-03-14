using System;
using System.IO;
using System.Net;
using System.Text;

namespace Sharpbrake.Client.Tests.Mocks
{
    /// <summary>
    /// Implementation of <see cref="IHttpResponse"/> with configurable status code and JSON response.
    /// </summary>
    public class FakeHttpResponse : IHttpResponse, IDisposable
    {
        private MemoryStream responseStream;

        public HttpStatusCode StatusCode { get; set; }

        public string ResponseJson { get; set; }

        public Stream GetResponseStream()
        {
            // setup in-memory stream with desired response (JSON string)
            var bytes = Encoding.UTF8.GetBytes(ResponseJson);
            responseStream = new MemoryStream();
            responseStream.Write(bytes, 0, bytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);
            return responseStream;
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
                if (responseStream != null)
                    responseStream.Dispose();
            }
        }
    }
}
