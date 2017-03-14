using Xunit;

namespace Sharpbrake.Client.IntegrationTests
{
    /// <summary>
    /// HttpServer fixture for different framework versions.
    /// </summary>
#if NET35
    public class HttpServerFixture : IUseFixture<HttpServer>
#else
    public class HttpServerFixture : IClassFixture<HttpServer>
#endif
    {
#if NET35
        public void SetFixture(HttpServer data)
        {
            // All initialization stuff is performed in HttpServer constructor.
            // Current method is for satisfying IUseFixture interface used in xUnit before v2 (the only way to support .NET 3.5).
        }
#endif
    }
}
