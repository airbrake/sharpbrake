using System.Collections.Generic;

namespace Sharpbrake.Client.Tests.Mocks
{
    /// <summary>
    /// Implementation of <see cref="IHttpContext"/> with empty properties.
    /// </summary>
    public class FakeHttpContext : IHttpContext
    {
        public IDictionary<string, string> Session { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public IDictionary<string, string> EnvironmentVars { get; set; }
        public string UserAgent { get; set; }
        public string Url { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
    }
}
