using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RaidBot.Backend.API
{
    /// <summary>
    /// Create a user
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class McUserEditorData
    {
        /// <summary>
        /// Internal identifier for this user, if known
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// E-Mail address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Base-64 encoded profile picture.
        /// </summary>
        public string ProfilePictureBase64 { get; set; }

        /// <summary>
        /// Creation date for the record
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreationDateUTC { get; set; }

        /// <summary>
        /// Last moditification date
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModificationDateUTC { get; set; }

        /// <summary>
        /// Is the user locked out?
        /// </summary>
        public bool LockoutEnabled { get; set; }

        /// <summary>
        /// Set only upon create of a new user.
        /// </summary>
        public string Password { get; set; }

        public bool IsAdministrator { get; set; }

        public bool IsFinance { get; set; }

        public bool IsStaff { get; set; }

        public bool IsVisitor { get; set; }
    }
}
