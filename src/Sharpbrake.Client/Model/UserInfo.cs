using Newtonsoft.Json;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Represents user specific info.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// If applicable, the current user's ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// If applicable, the current user's username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// If applicable, the current user's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
