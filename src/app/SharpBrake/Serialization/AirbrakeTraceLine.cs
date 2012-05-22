using System;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
    /// <summary>
    /// Required. This element can occur more than once. Each line element describes one
    /// code location or frame in the backtrace when the error occurred, and requires
    /// @file and @number attributes. If the location includes a method or function, the
    /// @method attribute should be used.
    /// </summary>
    [XmlRoot("line")]
    public class AirbrakeTraceLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeTraceLine"/> class.
        /// </summary>
        [Obsolete("Don't use, only for serialization.", true)]
        public AirbrakeTraceLine()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeTraceLine"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="lineNumber">The line number.</param>
        public AirbrakeTraceLine(string file, int lineNumber)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            File = file;
            LineNumber = lineNumber;
        }


        /// <summary>
        /// Gets or sets the file the error occurred in.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        [XmlAttribute("file")]
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the line number of the file in which the error occurred.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        [XmlAttribute("number")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the method in which the error occurred.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        [XmlAttribute("method")]
        public string Method { get; set; }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0} [{1}]: {2}", Method, File, LineNumber);
        }
    }
}