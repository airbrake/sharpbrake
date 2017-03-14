using Sharpbrake.Client;

namespace Sharpbrake.Http.Middleware
{
    /// <summary>
    /// Provides access to the Airbrake notifier used in the middleware.
    /// </summary>
    public interface IAirbrakeFeature
    {
        /// <summary>
        /// Gets an underlying notifier used by the Airbrake ASP.NET Core middleware.
        /// </summary>
        AirbrakeNotifier GetNotifier();
    }
}
