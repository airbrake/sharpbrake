using System;
using System.IO;
using System.Net;
using System.Text;

namespace Sharpbrake.Client.Tests.Mocks
{
    /// <summary>
    /// Implementation of <see cref="IHttpRequest"/> with in-memory request stream and
    /// configurable IHttpRequest implementation.
    /// </summary>
    public class FakeHttpRequest : IHttpRequest, IDisposable
    {
        public string ContentType { get; set; }
        public string Accept { get; set; }
        public string Method { get; set; }
        public IWebProxy Proxy { get; set; }

        public Uri RequestUri
        {
            get { return requestUri; }
        }

        private readonly IHttpResponse httpResponse;
        private readonly Stream requestStream;
        private readonly StringBuilder contentBuilder;
        private readonly Uri requestUri;

        public bool IsFaultedGetRequestStream { get; set; }
        public bool IsCanceledGetRequestStream { get; set; }

        public bool IsFaultedGetResponse { get; set; }
        public bool IsCanceledGetResponse { get; set; }

        public FakeHttpRequest(IHttpResponse response)
        {
            httpResponse = response;
            contentBuilder = new StringBuilder();
            requestStream = new CapturedMemoryStream(contentBuilder);
            requestUri = new Uri("https://airbrake.io");
        }

        public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            if (IsFaultedGetRequestStream)
                throw new Exception();

            Func<FakeHttpRequest> requestFunc = () => this;
            return requestFunc.BeginInvoke(callback, state);
        }

        public Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return ((FakeHttpRequest)asyncResult.AsyncState).requestStream;
        }

        public IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            if (IsFaultedGetResponse)
                throw new Exception();

            Func<FakeHttpRequest> requestFunc = () => this;
            return requestFunc.BeginInvoke(callback, state);
        }

        public IHttpResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return ((FakeHttpRequest)asyncResult.AsyncState).httpResponse;
        }

        public string GetRequestStreamContent()
        {
            return contentBuilder.ToString();
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
                if (requestStream != null)
                    requestStream.Dispose();
            }
        }
    }
}
