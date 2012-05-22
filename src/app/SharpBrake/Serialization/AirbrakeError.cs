using System;
using System.Reflection;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// Represents the Airbrake "error" element.
    /// </summary>
    [XmlInclude(typeof(AirbrakeTraceLine))]
    public class AirbrakeError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeError"/> class.
        /// </summary>
        [Obsolete("Don't use, only for serialization.", true)]
        public AirbrakeError()
        {
        }


        /// <summary>
        /// Required. Each line element describes one code location or frame in the backtrace when the
        /// error occurred.
        /// </summary>
        /// <value>
        /// Each line element describes one code location or frame in the backtrace when the error occurred.
        /// </value>
        [XmlArray("backtrace")]
        [XmlArrayItem("line")]
        public AirbrakeTraceLine[] Backtrace { get; set; }

        /// <summary>
        /// Required. The class name or type of error that occurred.
        /// </summary>
        /// <value>
        /// The class name or type of error that occurred.
        /// </value>
        [XmlElement("class")]
        public string Class { get; set; }

        /// <summary>
        /// Optional. A short message describing the error that occurred.
        /// </summary>
        /// <value>
        /// A short message describing the error that occurred.
        /// </value>
        [XmlElement("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the method that had the <c>catch</c> which created this <see cref="AirbrakeError"/>.
        /// </summary>
        /// <value>
        /// The method that had the <c>catch</c> which created this <see cref="AirbrakeError"/>.
        /// </value>
        [XmlIgnore]
        internal MethodBase CatchingMethod { get; set; }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0} : {1}", GetType(), Message);
        }
    }
}