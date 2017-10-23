using System;

namespace Sharpbrake.Client.Tests.Mocks
{
    /// <summary>
    /// Implementation of <see cref="IHttpRequestHandler"/> based on <see cref="FakeHttpRequest"/>.
    /// </summary>
    public class FakeHttpRequestHandler : IHttpRequestHandler, IDisposable
    {
        public FakeHttpRequest HttpRequest { get; }
        public FakeHttpResponse HttpResponse { get; }

        public FakeHttpRequestHandler()
        {
            HttpResponse = new FakeHttpResponse();
            HttpRequest = new FakeHttpRequest(HttpResponse);
        }

        public IHttpRequest Get()
        {
            return HttpRequest;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            HttpResponse?.Dispose();
            HttpRequest?.Dispose();
        }
    }
}
