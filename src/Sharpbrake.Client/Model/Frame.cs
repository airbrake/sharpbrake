using System.Runtime.Serialization;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Frame from the error.
    /// </summary>
    [DataContract]
    public class Frame
    {
        /// <summary>
        /// The full path of the file in this entry of the backtrace.
        /// </summary>
        [DataMember(Name = "file", EmitDefaultValue = false)]
        public string File { get; set; }

        /// <summary>
        /// The file's line number in this entry of the backtrace.
        /// </summary>
        [DataMember(Name = "line", EmitDefaultValue = false)]
        public int Line { get; set; }

        /// <summary>
        /// The line's column number in this entry of the backtrace.
        /// </summary>
        [DataMember(Name = "column", EmitDefaultValue = false)]
        public int Column { get; set; }

        /// <summary>
        /// When available, the function 
        /// or method name in this entry of the backtrace.
        /// </summary>
        [DataMember(Name = "function", EmitDefaultValue = false)]
        public string Function { get; set; }
    }
}
