using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McRaidInfo
    {
        public Guid UniqueID { get; set; }

        public string PublicID { get; set; }

        public McRaidDescription Raid { get; set; }

        public bool IsPublished { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastRefresh { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModificationTime { get; set; }
    }
}
