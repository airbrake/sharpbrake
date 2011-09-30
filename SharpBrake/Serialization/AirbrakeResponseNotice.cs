using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// The notice returned from Airbrake.
    /// </summary>
    [XmlRoot("notice")]
    public class AirbrakeResponseNotice
    {
        /// <summary>
        /// Gets the error id.
        /// </summary>
        /// <value>
        /// The error id.
        /// </value>
        [XmlElement("error-id")]
        public int ErrorId { get; set; }

        /// <summary>
        /// Gets the id of the created notice.
        /// </summary>
        /// <value>
        /// The id of the created notice.
        /// </value>
        [XmlElement("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets the URL of the created notice.
        /// </summary>
        /// <value>
        /// The URL of the created notice.
        /// </value>
        [XmlElement("url")]
        public string Url { get; set; }
    }
}