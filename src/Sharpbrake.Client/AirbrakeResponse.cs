using System.Runtime.Serialization;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Represents the status of request made to Airbrake endpoint.
    /// </summary>
    public enum RequestStatus
    {
        Success,
        Ignored,
        RequestError
    }

    /// <summary>
    /// Response from Airbrake.
    /// </summary>
    [DataContract]
    public sealed class AirbrakeResponse
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }
        [DataMember(Name = "url", EmitDefaultValue = false)]
        public string Url { get; set; }
        public RequestStatus Status { get; set; }
    }
}
