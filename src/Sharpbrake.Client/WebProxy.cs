#if NETSTANDARD1_4
using System;
using System.Net;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Implementation of <see cref="IWebProxy"/> for .NET Core 1.x.
    /// </summary>
    /// <remarks>
    /// There is no default implementation of IWebProxy in .NET Core 1.x.
    /// For more details see here: https://github.com/NuGet/Home/issues/1858#issuecomment-167651718
    /// </remarks>
    public class WebProxy : IWebProxy
    {
        private readonly Uri uri;

        public WebProxy(Uri uri)
        {
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination)
        {
            return uri;
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }
    }
}
#endif
