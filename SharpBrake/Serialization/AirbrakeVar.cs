using System;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// The params, session, and cgi-data elements can contain one or more var elements for each parameter or variable that was set when the error occurred. Each var element should have a @key attribute for the name of the variable, and element text content for the value of the variable.
    /// </summary>
    [XmlRoot("var")]
    public class AirbrakeVar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeVar"/> class.
        /// </summary>
        [Obsolete("Don't use. Only for serialization.", true)]
        public AirbrakeVar()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeVar"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public AirbrakeVar(string key, string value)
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
        [XmlText]
        public string Value { get; set; }
    }
}