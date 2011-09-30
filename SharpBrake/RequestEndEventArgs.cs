using System;
using System.Net;

namespace SharpBrake
{
    /// <summary>
    /// The event arguments passed to <see cref="RequestEndEventHandler"/>.
    /// </summary>
    [Serializable]
    public class RequestEndEventArgs : EventArgs
    {
        private readonly WebRequest request;
        private readonly AirbrakeResponse response;


        /// <summary>
        /// Initializes a new instance of the <see cref="RequestEndEventArgs"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="content">The body of the response.</param>
        public RequestEndEventArgs(WebRequest request, WebResponse response, string content)
        {
            this.request = request;
            this.response = new AirbrakeResponse(response, content);
        }


        /// <summary>
        /// Gets the request.
        /// </summary>
        public WebRequest Request
        {
            get { return this.request; }
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public AirbrakeResponse Response
        {
            get { return this.response; }
        }
    }
}