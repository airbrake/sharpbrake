using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HopSharp.Serialization
{
   /// <summary>
   /// The response retreived from HopToad.
   /// </summary>
   [Serializable]
   [XmlRoot("errors")]
   public class HoptoadResponse : IXmlSerializable
   {
      private readonly string content;
      private readonly long contentLength;
      private readonly string contentType;
      private readonly WebHeaderCollection headers;
      private readonly bool isFromCache;
      private readonly bool isMutuallyAuthenticated;
      private readonly Uri responseUri;
      private HoptoadResponseError[] errors;


      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadResponse"/> class.
      /// </summary>
      [Obsolete("Don't use. For serialization only.", true)]
      public HoptoadResponse()
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadResponse"/> class.
      /// </summary>
      /// <param name="response">The response.</param>
      /// <param name="content">The content.</param>
      public HoptoadResponse(WebResponse response, string content)
      {
         this.content = content;
         this.errors = new HoptoadResponseError[0];

         if (response == null)
         {
            // TryGet is needed because the default behavior of WebResponse is to throw NotImplementedException
            // when a method isn't overridden by a deriving class, instead of declaring the method as abstract.
            this.contentLength = response.TryGet(x => x.ContentLength);
            this.contentType = response.TryGet(x => x.ContentType);
            this.headers = response.TryGet(x => x.Headers);
            this.isFromCache = response.TryGet(x => x.IsFromCache);
            this.isMutuallyAuthenticated = response.TryGet(x => x.IsMutuallyAuthenticated);
            this.responseUri = response.TryGet(x => x.ResponseUri);
         }

         var serializer = new CleanXmlSerializer<HoptoadResponse>();
         var hoptoadResponse = serializer.FromXml(content);

         if (hoptoadResponse != null)
            this.errors = hoptoadResponse.Errors;
      }


      /// <summary>
      /// Gets the content.
      /// </summary>
      [XmlIgnore]
      public string Content
      {
         get { return this.content; }
      }

      /// <summary>
      /// Gets the length of the content.
      /// </summary>
      /// <value>
      /// The length of the content.
      /// </value>
      [XmlIgnore]
      public long ContentLength
      {
         get { return this.contentLength; }
      }

      /// <summary>
      /// Gets the type of the content.
      /// </summary>
      /// <value>
      /// The type of the content.
      /// </value>
      [XmlIgnore]
      public string ContentType
      {
         get { return this.contentType; }
      }

      /// <summary>
      /// Gets the errors.
      /// </summary>
      [XmlArray("errors")]
      [XmlArrayItem("error")]
      public HoptoadResponseError[] Errors
      {
         get { return this.errors; }
      }

      /// <summary>
      /// Gets the headers.
      /// </summary>
      [XmlIgnore]
      public WebHeaderCollection Headers
      {
         get { return this.headers; }
      }

      /// <summary>
      /// Gets a value indicating whether this instance is from cache.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is from cache; otherwise, <c>false</c>.
      /// </value>
      [XmlIgnore]
      public bool IsFromCache
      {
         get { return this.isFromCache; }
      }

      /// <summary>
      /// Gets a value indicating whether this instance is mutually authenticated.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is mutually authenticated; otherwise, <c>false</c>.
      /// </value>
      [XmlIgnore]
      public bool IsMutuallyAuthenticated
      {
         get { return this.isMutuallyAuthenticated; }
      }

      /// <summary>
      /// Gets the response URI.
      /// </summary>
      [XmlIgnore]
      public Uri ResponseUri
      {
         get { return this.responseUri; }
      }


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
         this.errors = BuildErrorsFrom(reader).ToArray();
      }


      /// <summary>
      /// Converts an object into its XML representation.
      /// </summary>
      /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. 
      ///                 </param>
      public void WriteXml(XmlWriter writer)
      {
         // We're never going to serialize the response from HopToad, only deserialize, so only ReadXml is implemented.
      }


      private static IEnumerable<HoptoadResponseError> BuildErrorsFrom(XmlReader reader)
      {
         while (reader.Read())
         {
            switch (reader.NodeType)
            {
               case XmlNodeType.Element:
                  if (reader.LocalName == "error")
                     yield return new HoptoadResponseError(reader.ReadElementContentAsString());
                  break;
            }
         }
      }
   }
}