using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Object describing the error that occurred.
    /// </summary>
    [DataContract]
    public class ErrorEntry
    {
        /// <summary>
        /// The class name or type of error that occurred.
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }

        /// <summary>
        /// A short message describing the error that occurred.
        /// </summary>
        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// An array of objects describing each line of the error's backtrace.
        /// </summary>
        [DataMember(Name = "backtrace", EmitDefaultValue = false)]
        public IList<Frame> Backtrace { get; set; }
    }
}
