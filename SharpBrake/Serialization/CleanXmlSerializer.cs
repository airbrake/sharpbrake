using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// Wraps XML serialization and doesn't generate processing instructions on document start 
    /// as well as xsi and xsd namespace definitions
    /// </summary>
    public abstract class CleanXmlSerializer
    {
        private readonly XmlSerializerNamespaces namespaces;
        private readonly XmlSerializer serializer;


        /// <summary>
        /// Initializes a new instance of the <see cref="CleanXmlSerializer&lt;TRoot&gt;"/> class.
        /// </summary>
        protected CleanXmlSerializer(Type type)
        {
            //Create our own namespaces for the output
            this.namespaces = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            this.namespaces.Add("", "");
            this.serializer = new XmlSerializer(type);
        }


        /// <summary>
        /// Serializes the <paramref name="source"/> to XML.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The <paramref name="source"/> serialized to XML.
        /// </returns>
        protected string ToXml(object source)
        {
            using (var writer = new StringWriter())
            {
                using (var xmlWriter = new XmlTextWriterFormattedNoDeclaration(writer))
                {
                    this.serializer.Serialize(xmlWriter, source, this.namespaces);
                    return writer.ToString();
                }
            }
        }


        /// <summary>
        /// Serializes the <paramref name="source"/> to XML.
        /// </summary>
        /// <typeparam name="T">The type of the object that is to be serialized.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The <paramref name="source"/> serialized to XML.
        /// </returns>
        public static string ToXml<T>(T source)
        {
            CleanXmlSerializer<T> serializer = new CleanXmlSerializer<T>();
            return serializer.ToXml(source);
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
        }

        #endregion
    }
}