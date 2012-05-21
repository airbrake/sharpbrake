using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using NUnit.Framework;

namespace SharpBrake.Tests
{
    public static class AirbrakeValidator
    {
        public static void ValidateSchema(string xml)
        {
            var schema = GetXmlSchema();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            settings.Schemas.Add(schema);

            using (var reader = new StringReader(xml))
            using (var xmlReader = new XmlTextReader(reader))
            using (var validator = XmlReader.Create(xmlReader, settings))
            while (validator.Read());
        }


        private static XmlSchema GetXmlSchema()
        {
            Type clientType = typeof(AirbrakeClient);
            const string xsd = "airbrake_2_2.xsd";

            using (Stream schemaStream = clientType.Assembly.GetManifestResourceStream(clientType, xsd))
            {
                if (schemaStream == null)
                    Assert.Fail("{0}.{1} not found.", clientType.Namespace, xsd);

                return XmlSchema.Read(schemaStream, null);
            }
        }
    }
}