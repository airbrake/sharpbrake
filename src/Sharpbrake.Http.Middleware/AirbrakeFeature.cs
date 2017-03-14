using Sharpbrake.Client;

namespace Sharpbrake.Http.Middleware
{
    /// <summary>
    /// Implementation of the <see cref="IAirbrakeFeature"/> interface
    /// that exposes a notifier passed to the constructor.
    /// </summary>
    public class AirbrakeFeature : IAirbrakeFeature
    {
        private AirbrakeNotifier notifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeFeature"/> class.
        /// </summary>
        public AirbrakeFeature(AirbrakeNotifier notifier)
        {
            this.notifier = notifier;
        }

        /// <summary>
        /// Gets a notifier passed to the constructor of the <see cref="AirbrakeFeature"/> class.
        /// </summary>
        public AirbrakeNotifier GetNotifier()
        {
            return notifier;
        }
    }
}
