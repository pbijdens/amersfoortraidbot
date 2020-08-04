using Botje.DB;
using System;

namespace RaidBot.Backend.Bot.PokemonRaidBot.Entities
{
    public class ChannelUpdateMessage : IAtom
    {
        public Guid UniqueID { get; set; }

        public long ChannelID { get; set; }

        public long MessageID { get; set; }

        public string Hash { get; set; }

        public DateTime LastModificationDate { get; set; }
    }
}
