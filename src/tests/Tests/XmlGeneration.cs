using System.Xml.Serialization;

using NUnit.Framework;

using SharpBrake.Serialization;

namespace SharpBrake.Tests
{
    [XmlRoot("notice", Namespace = "")]
    public class TestNotice
    {
        [XmlElement("api-key")]
        public string ApiKey { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }
    }

    [TestFixture]
    public class CleanXmlGeneration
    {
        [Test]
        public void Xml_contains_no_fluff()
        {
            var notice = new TestNotice
            {
                ApiKey = "123456",
                Version = "2.0"
            };

            var serializer = new CleanXmlSerializer<TestNotice>();
            string xml = serializer.ToXml(notice);

            const string expected = @"<notice version=""2.0"">
  <api-key>123456</api-key>
</notice>";
            Assert.AreEqual(expected, xml);
        }
    }
}