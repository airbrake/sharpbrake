using System;
using System.Xml.Serialization;

namespace HopSharp.Serialization
{
   /// <summary>
   /// Optional. If this error occurred during an HTTP request, the children of this element can be used to describe the request that caused the error.
   /// </summary>
   [XmlInclude(typeof(HoptoadVar))]
   public class HoptoadRequest
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadRequest"/> class.
      /// </summary>
      [Obsolete("Don't use. Only for serialization.", true)]
      public HoptoadRequest()
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadRequest"/> class.
      /// </summary>
      /// <param name="url">The URL.</param>
      /// <param name="component">The component.</param>
      public HoptoadRequest(string url, string component)
      {
         Url = url;
         Component = component;
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="HoptoadRequest"/> class.
      /// </summary>
      /// <param name="url">The URL.</param>
      /// <param name="component">The component.</param>
      public HoptoadRequest(Uri url, string component)
      {
         Url = url == null ? null : url.ToString();
         Component = component;
      }


      /// <summary>
      /// Optional. The action in which the error occurred. If each request is routed to a controller action, this should be set here. Otherwise, this can be set to a method or other request subcategory.
      /// </summary>
      /// <value>
      /// The action in which the error occurred. If each request is routed to a controller action, this should be set here. Otherwise, this can be set to a method or other request subcategory.
      /// </value>
      [XmlElement("action")]
      public string Action { get; set; }

      /// <summary>
      /// Optional. A list of var elements describing CGI variables from the request, such as SERVER_NAME and REQUEST_URI.
      /// </summary>
      /// <value>
      /// A list of var elements describing CGI variables from the request, such as SERVER_NAME and REQUEST_URI.
      /// </value>
      [XmlArray("cgi-data")]
      [XmlArrayItem("var")]
      public HoptoadVar[] CgiData { get; set; }

      /// <summary>
      /// Required only if there is a request element. The component in which the error occurred. In model-view-controller frameworks like Rails, this should be set to the controller. Otherwise, this can be set to a route or other request category.
      /// </summary>
      /// <value>
      /// The component in which the error occurred. In model-view-controller frameworks like Rails, this should be set to the controller. Otherwise, this can be set to a route or other request category.
      /// </value>
      [XmlElement("component")]
      public string Component { get; set; }

      /// <summary>
      /// Optional. A list of var elements describing request parameters from the query string, POST body, routing, and other inputs.
      /// </summary>
      /// <value>
      /// A list of var elements describing request parameters from the query string, POST body, routing, and other inputs.
      /// </value>
      [XmlArray("params")]
      [XmlArrayItem("var")]
      public HoptoadVar[] Params { get; set; }

      /// <summary>
      /// Optional. A list of var elements describing session variables from the request.
      /// </summary>
      /// <value>
      /// A list of var elements describing session variables from the request.
      /// </value>
      [XmlArray("session")]
      [XmlArrayItem("var")]
      public HoptoadVar[] Session { get; set; }

      /// <summary>
      /// Required only if there is a request element. The URL at which the error occurred.
      /// </summary>
      /// <value>
      /// The URL at which the error occurred.
      /// </value>
      [XmlElement("url")]
      public string Url { get; set; }
   }
}