using System.Xml.Serialization;

namespace HopSharp.Serialization
{
    /// <summary>
    /// The class that identifies HopSharp as the library sending errors to Hoptoad.
    /// </summary>
    public class HoptoadNotifier
    {
        /// <summary>
        /// Required. The name of the notifier client submitting the request, such as "hoptoad4j" or "rack-hoptoad."
        /// </summary>
        /// <value>
        /// The name of the notifier client submitting the request, such as "hoptoad4j" or "rack-hoptoad."
        /// </value>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Required. The version number of the notifier client submitting the request.
        /// </summary>
        /// <value>
        /// The version number of the notifier client submitting the request.
        /// </value>
        [XmlElement("version")]
        public string Version { get; set; }

        /// <summary>
        /// Required. A URL at which more information can be obtained concerning the notifier client.
        /// </summary>
        /// <value>
        /// A URL at which more information can be obtained concerning the notifier client.
        /// </value>
        [XmlElement("url")]
        public string Url { get; set; }
    }
}