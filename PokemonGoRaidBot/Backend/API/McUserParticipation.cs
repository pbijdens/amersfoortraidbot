using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McUserParticipation
    {
        public McTelegramUser User { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UtcWhen { get; set; }

        public int Extra { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime UtcArrived { get; set; }
    }
}
