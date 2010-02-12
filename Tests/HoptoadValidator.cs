using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace Tests
{
	public class HoptoadValidator
	{
		public static void ValidateSchema(string xml)
		{
			var schemaStream = typeof(HoptoadValidator).Assembly.GetManifestResourceStream("Tests.hoptoad_2_0.xsd");
			var schema = XmlSchema.Read(schemaStream, (sender, args) => { });

			var reader = new StringReader(xml);
			var xmlReader = new XmlTextReader(reader);

#pragma warning disable 0618
			var validator = new XmlValidatingReader(xmlReader);
#pragma warning restore 0618

			var errorBuffer = new StringBuilder();
			validator.ValidationEventHandler += (sender, args) => {
				errorBuffer.AppendLine(args.Message);
			};

			validator.Schemas.Add(schema);
			while (validator.Read())
			{
			}

			if (errorBuffer.ToString().Length > 0)
				Assert.Fail(errorBuffer.ToString());
		}
	}
}