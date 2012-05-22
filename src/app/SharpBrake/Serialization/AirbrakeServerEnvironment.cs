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
            Hostname = Environment.MachineName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeServerEnvironment"/> class.
        /// </summary>
        [Obsolete("Don't use. Only for serialization.", true)]
        public AirbrakeServerEnvironment()
        {
        }


        /// <summary>
        /// Optional. The version of the application that this error came from.
        /// If the App Version is set on the project, then errors older than the project's app version will be ignored.
        /// This version field uses "Semantic Versioning" style versioning.
        /// </summary>
        /// <value>
        /// The app version.
        /// </value>
        [XmlElement("app-version", Order = 2)]
        public string AppVersion { get; set; }

        /// <summary>
        /// Required. The name of the server environment in which the error occurred, such as "staging" or "production."
        /// </summary>
        /// <value>
        /// The name of the environment.
        /// </value>
        [XmlElement("environment-name", Order = 1)]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Optional. The name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        [XmlElement("hostname", Order = 3)]
        public string Hostname { get; set; }

        /// <summary>
        /// Optional. The path to the project in which the error occurred, such as RAILS_ROOT or DOCUMENT_ROOT.
        /// </summary>
        /// <value>
        /// The project root.
        /// </value>
        [XmlElement("project-root", Order = 0)]
        public string ProjectRoot { get; set; }
    }
}