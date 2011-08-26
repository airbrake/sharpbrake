using System;
using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
   /// <summary>
   /// Contains information about the environment in which the error occurred.
   /// </summary>
    public class AirbrakeServerEnvironment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeServerEnvironment"/> class.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        public AirbrakeServerEnvironment(string environmentName)
        {
           if (environmentName == null)
              throw new ArgumentNullException("environmentName");

           EnvironmentName = environmentName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeServerEnvironment"/> class.
        /// </summary>
        [Obsolete("Don't use. Only for serialization.", true)]
        public AirbrakeServerEnvironment()
        {
        }


        /// <summary>
        /// Optional. The path to the project in which the error occurred, such as RAILS_ROOT or DOCUMENT_ROOT.
        /// </summary>
        /// <value>
        /// The project root.
        /// </value>
        [XmlElement("project-root")]
        public string ProjectRoot { get; set; }

        /// <summary>
        /// Required. The name of the server environment in which the error occurred, such as "staging" or "production."
        /// </summary>
        /// <value>
        /// The name of the environment.
        /// </value>
        [XmlElement("environment-name")]
        public string EnvironmentName { get; set; }
    }
}