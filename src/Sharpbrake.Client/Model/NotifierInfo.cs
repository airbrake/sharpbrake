using System.Reflection;
using System.Runtime.Serialization;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Object that describes the current notifier library.
    /// </summary>
    [DataContract]
    public class NotifierInfo
    {
        /// <summary>
        /// The name of the notifier client submitting the request.
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name
        {
            get => "sharpbrake";
            private set { }
        }

        /// <summary>
        /// The version number of the notifier client 
        /// submitting the request, e.g. "1.2.3".
        /// </summary>
        [DataMember(Name = "version", EmitDefaultValue = false)]
        public string Version
        {
            get
            {
                var version = typeof(NotifierInfo).GetTypeInfo().Assembly.GetName().Version;
                // in the Version class Microsoft uses the next versioning schema: major.minor[.build[.revision]]
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            private set { }
        }

        /// <summary>
        /// A URL at which more information can be obtained concerning the notifier client.
        /// </summary>
        [DataMember(Name = "url", EmitDefaultValue = false)]
        public string Url
        {
            get => "https://github.com/airbrake/sharpbrake";
            private set { }
        } 
    }
}
