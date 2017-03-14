using System.Collections.Generic;

namespace Sharpbrake.Client
{
    /// <summary>
    /// An interface that provides HTTP-related properties for notifier.
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// Session variables.
        /// </summary>
        IDictionary<string, string> Session { get; set; }

        /// <summary>
        /// Parameters which usually take from HTML form.
        /// </summary>
        IDictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Environment variables which usually take from HTML headers.
        /// </summary>
        IDictionary<string, string> EnvironmentVars { get; set; }

        /// <summary>
        /// Browser user agent.
        /// </summary>
        string UserAgent { get; set; }

        /// <summary>
        /// URL for current request.
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// User's id.
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// User's email.
        /// </summary>
        string UserEmail { get; set; }

        /// <summary>
        /// User's name.
        /// </summary>
        string UserName { get; set; }
    }
}
