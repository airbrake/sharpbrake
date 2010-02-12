using System.Xml.Serialization;

namespace HopSharp.Serialization
{
	public class HoptoadServerEnvironment
	{
		[XmlElement("project-root")]
		public string ProjectRoot { get; set; }
		[XmlElement("environment-name")]
		public string EnvironmentName { get; set; }
	}
}