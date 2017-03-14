
namespace Sharpbrake.Client.Impl
{
    /// <summary>
    /// Defines HTTP request handler based on the <see cref="HttpWebRequest"/> class.
    /// </summary>
    public class HttpRequestHandler : IHttpRequestHandler
    {
        private readonly string projectId;
        private readonly string projectKey;
        private readonly string host;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestHandler"/> class.
        /// </summary>
        public HttpRequestHandler(string projectId, string projectKey, string host = null)
        {
            this.projectId = projectId;
            this.projectKey = projectKey;
            this.host = host;
        }

        /// <summary>
        /// Gets implementation of <see cref="IHttpRequest"/> based on the <see cref="HttpWebRequest"/> class.
        /// </summary>
        public IHttpRequest Get()
        {
            return HttpWebRequest.Create(Utils.GetRequestUri(projectId, projectKey, host));
        }
    }
}
