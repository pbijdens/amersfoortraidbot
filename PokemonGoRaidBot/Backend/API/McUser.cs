using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McUser
    {
        /// <summary>
        /// Internal identifier for this user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// E-Mail address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Display name of the user/
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
    }
}
