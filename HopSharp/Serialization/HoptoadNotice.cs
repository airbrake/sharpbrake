using System.Xml.Serialization;

namespace HopSharp.Serialization
{
    [XmlRoot("notice", Namespace = "")]
    public class HoptoadNotice
    {
        public HoptoadNotice()
        {
            Version = "2.0";
        }

        [XmlElement("api-key")]
        public string ApiKey { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("error")]
        public HoptoadError Error { get; set; }

        [XmlElement("notifier")]
        public HoptoadNotifier Notifier { get; set; }

        [XmlElement("server-environment")]
        public HoptoadServerEnvironment ServerEnvironment { get; set; }
    }
}