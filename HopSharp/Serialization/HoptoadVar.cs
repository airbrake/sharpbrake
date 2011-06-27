using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HopSharp.Serialization
{
   /// <summary>
   /// The params, session, and cgi-data elements can contain one or more var elements for each parameter or variable that was set when the error occurred. Each var element should have a @key attribute for the name of the variable, and element text content for the value of the variable.
   /// </summary>
   [XmlRoot("var")]
   public class HoptoadVar : IXmlSerializable
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadVar"/> class.
      /// </summary>
      [Obsolete("Don't use. Only for serialization.", true)]
      public HoptoadVar()
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadVar"/> class.
      /// </summary>
      /// <param name="key">The key.</param>
      /// <param name="value">The value.</param>
      public HoptoadVar(string key, string value)
      {
         Key = key;
         Value = value;
      }


      /// <summary>
      /// Required. The key of the var, like <c>SERVER_NAME</c> or <c>REQUEST_URI</c>.
      /// </summary>
      /// <value>
      /// The key of the var, like <c>SERVER_NAME</c> or <c>REQUEST_URI</c>.
      /// </value>
      [XmlAttribute("key")]
      public string Key { get; set; }

      /// <summary>
      /// Gets or sets the value.
      /// </summary>
      /// <value>
      /// The value.
      /// </value>
      [XmlIgnore]
      public string Value { get; set; }


      /// <summary>
      /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
      /// </summary>
      /// <returns>
      /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
      /// </returns>
      public XmlSchema GetSchema()
      {
         return null;
      }


      /// <summary>
      /// Generates an object from its XML representation.
      /// </summary>
      /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. 
      ///                 </param>
      public void ReadXml(XmlReader reader)
      {
         // No need to deserialize HoptoadVar, so only WriteXml is implemented.
      }


      /// <summary>
      /// Converts an object into its XML representation.
      /// </summary>
      /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. 
      ///                 </param>
      public void WriteXml(XmlWriter writer)
      {
         // The <var/> element is already written, so we only need to fill in its contents.

         writer.WriteAttributeString("key", Key);
         writer.WriteString(Value);
      }
   }
}