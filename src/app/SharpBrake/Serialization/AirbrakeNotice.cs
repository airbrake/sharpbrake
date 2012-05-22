using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// The root notice class that encapsulate the error being sent to Airbrake.
    /// </summary>
    [XmlRoot("notice", Namespace = "")]
    public class AirbrakeNotice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeNotice"/> class.
        /// For API Version 2.2
        /// </summary>
        public AirbrakeNotice()
        {
            Version = "2.2";
        }


        /// <summary>
        /// Required. The API key for the project that this error belongs to. The API key can be found by viewing the edit project form on the Airbrake site.
        /// </summary>
        /// <value>
        /// The API key for the project that this error belongs to. The API key can be found by viewing the edit project form on the Airbrake site.
        /// </value>
        [XmlElement("api-key")]
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        [XmlElement("error")]
        public AirbrakeError Error { get; set; }

        /// <summary>
        /// Gets or sets the notifier.
        /// </summary>
        /// <value>
        /// The notifier.
        /// </value>
        [XmlElement("notifier")]
        public AirbrakeNotifier Notifier { get; set; }

        /// <summary>
        /// Optional. If this error occurred during an HTTP request, the children of this element can be used to describe the request that caused the error.
        /// </summary>
        /// <value>
        /// The children of this element can be used to describe the request that caused the error.
        /// </value>
        [XmlElement("request")]
        public AirbrakeRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the server environment.
        /// </summary>
        /// <value>
        /// The server environment.
        /// </value>
        [XmlElement("server-environment")]
        public AirbrakeServerEnvironment ServerEnvironment { get; set; }

        /// <summary>
        /// Required. The version of the API being used. Should be set to "2.2".
        /// </summary>
        /// <value>
        /// The version of the API being used. Should be set to "2.2".
        /// </value>
        [XmlAttribute("version")]
        public string Version { get; set; }
    }
}