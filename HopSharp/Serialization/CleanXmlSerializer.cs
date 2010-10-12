using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HopSharp.Serialization
{
    //Wrap XML serialization and do not generate processing instructions on document start 
    //as well as xsi and xsd namespace definitions
    public class CleanXmlSerializer<TRoot>
    {
        private readonly XmlSerializerNamespaces _namespaces;

        public CleanXmlSerializer()
        {
            //Create our own namespaces for the output
            _namespaces = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            _namespaces.Add("", "");
        }

        public string ToXml(TRoot source)
        {
            var writer = new StringWriter();
            var xmlWriter = new XmlTextWriterFormattedNoDeclaration(writer);

            var serializer = new XmlSerializer(typeof (TRoot));
            serializer.Serialize(xmlWriter, source, _namespaces);

            return writer.GetStringBuilder().ToString();
        }

        #region Nested type: XmlTextWriterFormattedNoDeclaration

        private class XmlTextWriterFormattedNoDeclaration : XmlTextWriter
        {
            public XmlTextWriterFormattedNoDeclaration(TextWriter w)
                : base(w)
            {
                Formatting = Formatting.Indented;
            }

            public override void WriteStartDocument()
            {
            }

            // suppress
        }

        #endregion
    }
}