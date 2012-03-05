namespace SharpBrake
{
    /// <summary>
    /// Contains the error message returned from Airbrake.
    /// </summary>
    public class AirbrakeResponseError
    {
        private readonly string message;


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeResponseError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public AirbrakeResponseError(string message)
        {
            this.message = message;
        }


        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message
        {
            get { return this.message; }
        }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.message;
        }
    }
}