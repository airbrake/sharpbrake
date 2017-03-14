using Newtonsoft.Json;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Frame from the error.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// The full path of the file in this entry of the backtrace.
        /// </summary>
        [JsonProperty("file")]
        public string File { get; set; }

        /// <summary>
        /// The file's line number in this entry of the backtrace.
        /// </summary>
        [JsonProperty("line")]
        public int Line { get; set; }

        /// <summary>
        /// The line's column number in this entry of the backtrace.
        /// </summary>
        [JsonProperty("column")]
        public int Column { get; set; }

        /// <summary>
        /// When available, the function 
        /// or method name in this entry of the backtrace.
        /// </summary>
        [JsonProperty("function")]
        public string Function { get; set; }
    }
}
