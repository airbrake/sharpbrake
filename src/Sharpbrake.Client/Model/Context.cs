using Newtonsoft.Json;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// An object describing additional context info about OS,
    /// notifier, environment and others, for this error.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// An object describing the notifier client library.
        /// </summary>
        [JsonProperty("notifier")]
        public NotifierInfo Notifier { get; set; }

        /// <summary>
        /// Details of the operating system on which the error occurred.
        /// </summary>
        [JsonProperty("os")]
        public string Os { get; set; }

        /// <summary>
        /// The hostname of the server on which the error occurred.
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Describe the language on which
        /// the error occurred, e.g. "Ruby 2.1.1".
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Describe the application version, e.g. "v1.2.3".
        /// </summary>
        [JsonProperty("environment")]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The action in which the error occurred.
        /// If each request is routed to a controller action,
        /// this should be set here. Otherwise,
        /// this can be set to a method or other request subcategory.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// The component or module in which the error occurred.
        /// In MVC frameworks like Rails,  this should be set to the controller.
        /// Otherwise, this can be set to a route or other request category.
        /// </summary>
        [JsonProperty("component")]
        public string Component { get; set; }

        /// <summary>
        /// Describe the application version, e.g. "v1.2.3".
        /// </summary>
        [JsonProperty("version")]
        public string AppVersion { get; set; }

        /// <summary>
        /// The application's URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// The requesting browser's full user-agent string.
        /// </summary>
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// The application's root directory path.
        /// </summary>
        [JsonProperty("rootDirectory")]
        public string RootDirectory { get; set; }

        /// <summary>
        /// An optional object for information about user.
        /// </summary>
        [JsonProperty("user")]
        public UserInfo User { get; set; }

        /// <summary>
        /// Error severity.
        /// </summary>
        [JsonProperty("severity")]
        public string Severity { get; set; }
    }
}
