using Botje.Messaging.Models;
using RaidBot.Backend.Bot.PokemonRaidBot.Enums;
using System;

namespace RaidBot.Backend.Bot.PokemonRaidBot.Entities
{
    public class UserParticipation
    {
        public User User { get; set; }

        public DateTime UtcWhen { get; set; }

        public int Extra { get; set; }

        public DateTime UtcArrived { get; set; }

        public UserParticipationType Type { get; set; }

        public string AllValuesAsString() => $"{this.User?.UsernameOrName()};{this.UtcWhen};{this.UtcArrived};{this.Type};{this.Extra}";
    }
}
