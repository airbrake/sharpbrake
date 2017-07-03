using Newtonsoft.Json;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Object that describes the current notifier library.
    /// </summary>
    public class NotifierInfo
    {
        /// <summary>
        /// The name of the notifier client submitting the request.
        /// </summary>
        [JsonProperty("name")]
        public string Name => "sharpbrake";

        /// <summary>
        /// The version number of the notifier client 
        /// submitting the request, e.g. "1.2.3".
        /// </summary>
        [JsonProperty("version")]
        public string Version => "3.1.1";

        /// <summary>
        /// An URL at which more information can be obtained concerning the notifier client.
        /// </summary>
        [JsonProperty("url")]
        public string Url => "https://github.com/airbrake/sharpbrake";
    }
}
