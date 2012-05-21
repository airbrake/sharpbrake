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
        public AirbrakeVar(string key, object value)
        {
            Key = key;
            Value = value == null
                        ? null
                        : value.ToString();
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


        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            AirbrakeVar other = obj as AirbrakeVar;

            if (other == null)
                return false;

            return GetHashCode() == other.GetHashCode();
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 0;

            if (Key != null)
                hashCode += Key.GetHashCode();

            if (Value != null)
                hashCode += Value.GetHashCode();

            if (hashCode == 0)
                hashCode = base.GetHashCode();

            return hashCode;
        }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("[{0} : {1}]", Key, Value);
        }
    }
}