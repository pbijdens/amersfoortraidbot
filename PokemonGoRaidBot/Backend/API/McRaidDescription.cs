using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RaidBot.Backend.Bot.PokemonRaidBot.Entities;
using System;
using System.Collections.Generic;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McRaidDescription
    {
        public Guid UniqueID { get; set; }

        public string PublicID { get; set; }

        public McTelegramUser User { get; set; }

        public McLocation Location { get; set; }

        public String Address { get; set; }

        public string Raid { get; set; }

        public string Gym { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Team Alignment { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RaidUnlockTime { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RaidEndTime { get; set; }

        public int UpdateCount { get; set; }

        public string Remarks { get; set; }

        public long? TelegramMessageID { get; set; }

        public int Valor { get; set; }

        public int Mystic { get; set; }

        public int Instinct { get; set; }

        public int Unknown { get; set; }

        public int Maybe { get; set; }

        public List<McRaidPublication> Publications { get; set; }
    }

    public class McRaidPublication
    {
        public long ChannelID { get; set; }

        public long TelegramMessageID { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModificationTimeUTC { get; set; }
    }
}
