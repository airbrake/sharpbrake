using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
   /// <summary>
   /// Wraps XML serialization and doesn't generate processing instructions on document start 
   /// as well as xsi and xsd namespace definitions
   /// </summary>
   /// <typeparam name="TRoot">The type of the root.</typeparam>
    public class CleanXmlSerializer<TRoot>
    {
        private readonly XmlSerializerNamespaces _namespaces;
        private readonly XmlSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanXmlSerializer&lt;TRoot&gt;"/> class.
        /// </summary>
        public CleanXmlSerializer()
        {
            //Create our own namespaces for the output
            _namespaces = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            _namespaces.Add("", "");
            _serializer = new XmlSerializer(typeof(TRoot));
        }

        /// <summary>
        /// Serializes the <see cref="source"/> to XML.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The <paramref name="source"/> serialized to XML.
        /// </returns>
        public string ToXml(TRoot source)
        {
            using (var writer = new StringWriter())
            using (var xmlWriter = new XmlTextWriterFormattedNoDeclaration(writer))
            {
               this._serializer.Serialize(xmlWriter, source, _namespaces);
               return writer.ToString();
            }
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