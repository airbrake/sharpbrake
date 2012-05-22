using System;
using System.Linq;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// Optional. If this error occurred during an HTTP request, the children of this element can be used to describe the request that caused the error.
    /// </summary>
    [XmlInclude(typeof(AirbrakeVar))]
    public class AirbrakeRequest
    {
        private AirbrakeVar[] cgiData;
        private AirbrakeVar[] parameters;
        private AirbrakeVar[] session;


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeRequest"/> class.
        /// </summary>
        [Obsolete("Don't use. Only for serialization.", true)]
        public AirbrakeRequest()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeRequest"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="component">The component in which the error occurred. In model-view-controller frameworks like Rails, this should be set to the controller. Otherwise, this can be set to a route or other request category.</param>
        public AirbrakeRequest(string url, string component)
        {
            Url = url;
            Component = component;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeRequest"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="component">The component in which the error occurred. In model-view-controller frameworks like Rails, this should be set to the controller. Otherwise, this can be set to a route or other request category.</param>
        public AirbrakeRequest(Uri url, string component)
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
        public AirbrakeVar[] CgiData
        {
            get { return this.cgiData != null && this.cgiData.Any() ? this.cgiData : null; }
            set { this.cgiData = value; }
        }

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
        public AirbrakeVar[] Params
        {
            get { return this.parameters != null && this.parameters.Any() ? this.parameters : null; }
            set { this.parameters = value; }
        }

        /// <summary>
        /// Optional. A list of var elements describing session variables from the request.
        /// </summary>
        /// <value>
        /// A list of var elements describing session variables from the request.
        /// </value>
        [XmlArray("session")]
        [XmlArrayItem("var")]
        public AirbrakeVar[] Session
        {
            get { return this.session != null && this.session.Any() ? this.session : null; }
            set { this.session = value; }
        }

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