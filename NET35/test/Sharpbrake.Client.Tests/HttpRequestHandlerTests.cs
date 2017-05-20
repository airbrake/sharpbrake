using Sharpbrake.Client.Impl;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="HttpRequestHandler"/> class.
    /// </summary>
    public class HttpRequestHandlerTests
    {
        [Fact]
        public void Get_ShouldReturnDefaultHttpWebRequest()
        {
            var httpRequestHandler = new HttpRequestHandler("127348", "e2046ca6e4e9214b24ad252e3c99a0f6");
            var httpRequest = httpRequestHandler.Get();

            Assert.NotNull(httpRequest);
            Assert.IsType<HttpWebRequest>(httpRequest);
        }
    }
}
