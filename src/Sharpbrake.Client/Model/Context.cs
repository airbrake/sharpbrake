using System.Runtime.Serialization;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// An object describing additional context info about OS,
    /// notifier, environment and others, for this error.
    /// </summary>
    [DataContract]
    public class Context
    {
        /// <summary>
        /// An object describing the notifier client library.
        /// </summary>
        [DataMember(Name = "notifier", EmitDefaultValue = false)]
        public NotifierInfo Notifier { get; set; }

        /// <summary>
        /// Details of the operating system on which the error occurred.
        /// </summary>
        [DataMember(Name = "os", EmitDefaultValue = false)]
        public string Os { get; set; }

        /// <summary>
        /// The hostname of the server on which the error occurred.
        /// </summary>
        [DataMember(Name = "hostname", EmitDefaultValue = false)]
        public string Hostname { get; set; }

        /// <summary>
        /// Describe the language on which
        /// the error occurred, e.g. "Ruby 2.1.1".
        /// </summary>
        [DataMember(Name = "language", EmitDefaultValue = false)]
        public string Language { get; set; }

        /// <summary>
        /// Describe the application version, e.g. "v1.2.3".
        /// </summary>
        [DataMember(Name = "environment", EmitDefaultValue = false)]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The action in which the error occurred.
        /// If each request is routed to a controller action,
        /// this should be set here. Otherwise,
        /// this can be set to a method or other request subcategory.
        /// </summary>
        [DataMember(Name = "action", EmitDefaultValue = false)]
        public string Action { get; set; }

        /// <summary>
        /// The component or module in which the error occurred.
        /// In MVC frameworks like Rails,  this should be set to the controller.
        /// Otherwise, this can be set to a route or other request category.
        /// </summary>
        [DataMember(Name = "component", EmitDefaultValue = false)]
        public string Component { get; set; }

        /// <summary>
        /// Describe the application version, e.g. "v1.2.3".
        /// </summary>
        [DataMember(Name = "version", EmitDefaultValue = false)]
        public string AppVersion { get; set; }

        /// <summary>
        /// The application's URL.
        /// </summary>
        [DataMember(Name = "url", EmitDefaultValue = false)]
        public string Url { get; set; }

        /// <summary>
        /// The IP address of the user that triggered the notice.
        /// </summary>
        [DataMember(Name = "userAddr", EmitDefaultValue = false)]
        public string UserAddr { get; set; }
        
        /// <summary>
        /// The requesting browser's full user-agent string.
        /// </summary>
        [DataMember(Name = "userAgent", EmitDefaultValue = false)]
        public string UserAgent { get; set; }

        /// <summary>
        /// The application's root directory path.
        /// </summary>
        [DataMember(Name = "rootDirectory", EmitDefaultValue = false)]
        public string RootDirectory { get; set; }

        /// <summary>
        /// An optional object for information about user.
        /// </summary>
        [DataMember(Name = "user", EmitDefaultValue = false)]
        public UserInfo User { get; set; }

        /// <summary>
        /// Error severity.
        /// </summary>
        [DataMember(Name = "severity", EmitDefaultValue = false)]
        public string Severity { get; set; }
    }

    /// <summary>
    /// Severity allows to control the importance of every single error the notifier sends.
    /// Errors with a severity of debug, info, notice, or warning will not trigger error
    /// emails or integration notifications.
    /// </summary>
    public enum Severity
    {
        Debug,
        Info,
        Notice,
        Warning,
        Error,
        Critical,
        Alert,
        Emergency
    }
}
