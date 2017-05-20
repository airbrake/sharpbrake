
namespace Sharpbrake.Client
{
    public interface IHttpRequestHandler
    {
        /// <summary>
        /// Gets implementation of <see cref="IHttpRequest"/> for performing requests to the Airbrake endpoint.
        /// </summary>
        IHttpRequest Get();
    }
}
