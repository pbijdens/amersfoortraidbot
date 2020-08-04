using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RaidBot.Backend.Bot.PokemonRaidBot.Entities;
using System;
using System.Collections.Generic;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McRaidDetails
    {
        public Guid UniqueID { get; set; }

        public string PublicID { get; set; }

        public McRaidDescription Raid { get; set; }

        public bool IsPublished { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastRefresh { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastModificationTime { get; set; }

        public Dictionary<Team, List<McUserParticipation>> Participants { get; set; }

        public List<McTelegramUser> Rejected { get; set; }

        public List<McTelegramUser> Done { get; set; }

        public List<McTelegramUser> Maybe { get; set; }
    }
}
