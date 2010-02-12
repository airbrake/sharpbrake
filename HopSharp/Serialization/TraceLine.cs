using System.Xml.Serialization;

namespace HopSharp.Serialization
{
	[XmlRoot("line")]
	public class TraceLine
	{
		[XmlAttribute("file")]
		public string File { get; set; }
		[XmlAttribute("number")]
		public int LineNumber { get; set; }
		[XmlAttribute("method")]
		public string Method { get; set; }
	}
}