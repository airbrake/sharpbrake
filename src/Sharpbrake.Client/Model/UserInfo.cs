using System.Runtime.Serialization;

namespace Sharpbrake.Client.Model
{
    /// <summary>
    /// Represents user specific info.
    /// </summary>
    [DataContract]
    public class UserInfo
    {
        /// <summary>
        /// If applicable, the current user's ID.
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// If applicable, the current user's username.
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// If applicable, the current user's email address.
        /// </summary>
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }
    }
}
