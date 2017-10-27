#if NETSTANDARD1_4 || NETSTANDARD2_0
using System;
using System.Net;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Implementation of <see cref="IWebProxy"/> for .NET Core.
    /// </summary>
    /// <remarks>
    /// There is no default implementation of IWebProxy in .NET Core.
    /// For more details see here: https://github.com/NuGet/Home/issues/1858#issuecomment-167651718
    /// </remarks>
    public class WebProxy : IWebProxy
    {
        private readonly bool bypassOnLocal;
        private readonly Uri uri;

        public WebProxy(Uri uri, bool bypassOnLocal)
        {
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
            this.bypassOnLocal = bypassOnLocal;
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination)
        {
            return uri;
        }

        public bool IsBypassed(Uri host)
        {
            return bypassOnLocal;
        }
    }
}
#endif
