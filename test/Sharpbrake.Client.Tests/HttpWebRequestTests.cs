using System;
using System.Net;
using Xunit;
using HttpWebRequest = Sharpbrake.Client.Impl.HttpWebRequest;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="HttpWebRequest"/> class.
    /// </summary>
    public class HttpWebRequestTests
    {
        [Fact]
        public void Ctor_ShouldThrowExceptionIfRequestParamIsEmpty()
        {
            var exception = Record.Exception(() => new HttpWebRequest(null));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("request", ((ArgumentNullException)exception).ParamName);
        }

        [Fact]
        public void Create_ShouldThrowExceptionIfEndpointParamIsEmpty()
        {
            var exception = Record.Exception(() => HttpWebRequest.Create(""));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("endpoint", ((ArgumentNullException)exception).ParamName);
        }

        [Fact]
        public void Create_ShouldReturnDefaultHttpWebRequestInstanceWithProperEndpoint()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpRequest = HttpWebRequest.Create(endpoint);

            Assert.NotNull(httpRequest);
            Assert.True(httpRequest.RequestUri.ToString() == endpoint);
            Assert.IsType<HttpWebRequest>(httpRequest);
        }

        [Fact]
        public void RequestUri_ShouldGetRequestUriOfUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            var httpRequest = new HttpWebRequest(httpWebRequest);

            Assert.True(httpRequest.RequestUri == httpWebRequest.RequestUri);
        }

        [Fact]
        public void ContentType_ShouldGetContentTypeOfUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";

            var httpRequest = new HttpWebRequest(httpWebRequest);

            Assert.True(httpRequest.ContentType == httpWebRequest.ContentType);
        }

        [Fact]
        public void ContentType_ShouldSetContentTypeToUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            var httpRequest = new HttpWebRequest(httpWebRequest) { ContentType = "application/json"};

            Assert.True(httpRequest.ContentType == httpWebRequest.ContentType);
        }

        [Fact]
        public void Accept_ShouldGetAcceptOfUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.Accept = "application/json";

            var httpRequest = new HttpWebRequest(httpWebRequest);

            Assert.True(httpRequest.Accept == httpWebRequest.Accept);
        }

        [Fact]
        public void Accept_ShouldSetAcceptToUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            var httpRequest = new HttpWebRequest(httpWebRequest) { Accept = "application/json"};

            Assert.True(httpRequest.Accept == httpWebRequest.Accept);
        }

        [Fact]
        public void Method_ShouldGetMethodOfUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.Method = "POST";

            var httpRequest = new HttpWebRequest(httpWebRequest);

            Assert.True(httpRequest.Method == httpWebRequest.Method);
        }

        [Fact]
        public void Method_ShouldSetMethodToUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            var httpRequest = new HttpWebRequest(httpWebRequest) { Method = "POST"};

            Assert.True(httpRequest.Method == httpWebRequest.Method);
        }

        [Fact]
        public void Proxy_ShouldGetProxyOfUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.Proxy = new WebProxy(new Uri("http://proxy-example.com:9090"), true);

            var httpRequest = new HttpWebRequest(httpWebRequest);

            Assert.NotNull(httpRequest.Proxy);
            Assert.True(httpRequest.Proxy == httpWebRequest.Proxy);
        }

        [Fact]
        public void Proxy_ShouldSetProxyToUnderlyingImplementation()
        {
            const string endpoint = "https://api.airbrake.io/api/v3/projects/123456/notices?key=e0246db6e4e921b424ad252e3c99a0f6";

            var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(endpoint);
            var httpRequest = new HttpWebRequest(httpWebRequest) { Proxy = new WebProxy(new Uri("http://proxy-example.com:9090"), true) };

            Assert.NotNull(httpWebRequest.Proxy);
            Assert.True(httpRequest.Proxy == httpWebRequest.Proxy);
        }
    }
}
