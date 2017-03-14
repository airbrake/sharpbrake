using System;

namespace Sharpbrake.Client
{
    /// <summary>
    /// An interface that represents logging functionality.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs response from the Airbrake endpoint.
        /// </summary>
        void Log(AirbrakeResponse response);

        /// <summary>
        /// Logs exception occurred during call to the Airbrake endpoint.
        /// </summary>
        void Log(Exception exception);
    }
}
