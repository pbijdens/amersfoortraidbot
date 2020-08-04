using Botje.Messaging.Models;
using System;

namespace RaidBot.Backend.Bot.PokemonRaidBot.Entities
{
    public class UserParticipation
    {
        public User User { get; set; }
        public DateTime UtcWhen { get; set; }
        public int Extra { get; set; }
        public DateTime UtcArrived { get; set; }

        public string AllValuesAsString() => $"{User?.UsernameOrName()};{UtcWhen};{UtcArrived};{Extra}";
    }
}
