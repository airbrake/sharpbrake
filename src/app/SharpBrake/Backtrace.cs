using System.Collections.Generic;
using System.Reflection;
using SharpBrake.Serialization;

namespace SharpBrake
{
    /// <summary>
    /// Represents information about an error
    /// </summary>
    public class Backtrace
    {
        /// <summary>
        /// Message component
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Stack Trace of the exception
        /// </summary>
        public List<AirbrakeTraceLine> Trace { get; set; }

        /// <summary>
        /// Method where the exception occured at
        /// </summary>
        public MethodBase CatchingMethod { get; set; }
    }
}