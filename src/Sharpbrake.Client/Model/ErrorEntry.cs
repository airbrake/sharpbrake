using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Object describing the error that occurred.
    /// </summary>
    public class ErrorEntry
    {
        /// <summary>
        /// The class name or type of error that occurred.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// A short message describing the error that occurred.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// An array of objects describing each line of the error's backtrace.
        /// </summary>
        [JsonProperty("backtrace")]
        public IList<Frame> Backtrace { get; set; }
    }
}
