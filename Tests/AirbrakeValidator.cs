using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using NUnit.Framework;

namespace SharpBrake.Tests
{
    public class AirbrakeValidator
    {
        public static void ValidateSchema(string xml)
        {
            var schema = GetXmlSchema();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
            };

            var errorBuffer = new StringBuilder();

            settings.ValidationEventHandler += (sender, args) => errorBuffer.AppendLine(args.Message);
            settings.Schemas.Add(schema);

            using (var reader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(reader))
                {
                    using (var validator = XmlReader.Create(xmlReader, settings))
                    {
                        while (validator.Read())
                        {
                        }
                    }
                }
            }

            if (errorBuffer.Length > 0)
                Assert.Fail(errorBuffer.ToString());
        }


        private static XmlSchema GetXmlSchema()
        {
            const string xsd = "hoptoad_2_1.xsd";

            Type clientType = typeof(AirbrakeClient);
            XmlSchema schema;

            using (Stream schemaStream = clientType.Assembly.GetManifestResourceStream(clientType, xsd))
            {
                if (schemaStream == null)
                    Assert.Fail("{0}.{1} not found.", clientType.Namespace, xsd);

                schema = XmlSchema.Read(schemaStream, (sender, args) => {  });
            }
            return schema;
        }
    }
}