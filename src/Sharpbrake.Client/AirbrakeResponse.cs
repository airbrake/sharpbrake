
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
    public sealed class AirbrakeResponse
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public RequestStatus Status { get; set; }
    }
}
