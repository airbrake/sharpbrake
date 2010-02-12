using System.IO;
using System.Text;
using System.Xml.Serialization;
using HopSharp;
using NUnit.Framework;

namespace Tests
{
	[XmlRoot("notice", Namespace = "")]
	[XmlType(AnonymousType = true)]
	public class TestNotice
	{
		[XmlElement("api-key")]
		public string ApiKey
		{
			get;
			set;
		}

		[XmlAttribute("version")]
		public string Version
		{
			get;
			set;
		}
	}

	public class CleanXmlGeneration
	{
		public void Xml_contains_no_fluff()
		{
			var notice = new TestNotice { 
				ApiKey = "123456",
				Version = "2.0"
			};

			var serializer = new CleanXmlSerializer<TestNotice>();
			var xml = serializer.ToXml(notice);

			var expected = @"<notice version=""2.0"">
  <api-key>123456</api-key>
</notice>";
			Assert.AreEqual(expected, xml);
		}
	}
}