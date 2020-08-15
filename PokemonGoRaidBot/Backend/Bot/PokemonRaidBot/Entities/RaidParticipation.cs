using Botje.Core.Utils;
using Botje.DB;
using Botje.Messaging.Models;
using RaidBot.Backend.Bot.PokemonRaidBot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidBot.Backend.Bot.PokemonRaidBot.Entities
{
    public class RaidParticipation : IAtom
    {
        public static object Lock = new object(); // global lock, bit too broad should be per raid, but okay as long as we don't have hundreds of raids at a time.

        public Guid UniqueID { get; set; }

        public string PublicID { get; set; }

        public RaidDescription Raid { get; set; }

        public Dictionary<Team, List<UserParticipation>> Participants { get; set; }

        public List<User> Rejected { get; set; }

        public List<User> Done { get; set; }

        public List<User> Maybe { get; set; }

        public bool IsPublished { get; set; }

        public DateTime LastRefresh { get; set; }

        public DateTime LastModificationTime { get; set; }

        public RaidParticipation()
        {
            PublicID = ShortGuid.NewGuid().ToString();
            Participants = new Dictionary<Team, List<UserParticipation>>();
            foreach (Team team in Enum.GetValues(typeof(Team)).OfType<Team>())
            {
                Participants[team] = new List<UserParticipation>();
            }
            Rejected = new List<User>();
            Done = new List<User>();
            Maybe = new List<User>();
        }

        internal int NumberOfParticipants()
        {
            int result = 0;
            foreach (var entry in Participants.SelectMany(x => x.Value))
            {
                result += entry.Extra + 1;
            }
            return result;
        }

        public string AllValuesAsString()
        {
            string participants = string.Join(",", (Participants?.Select(x => string.Concat(x.Key.ToString(), String.Join("|", x.Value?.Select(v => v.AllValuesAsString()).ToList() ?? new List<string>()))).ToList() ?? new List<string>()));
            string rejected = string.Join(",", (Rejected?.Select(x => x.UsernameOrName()).ToList() ?? new List<string>()));
            string maybe = string.Join(",", (Maybe?.Select(x => x.UsernameOrName()).ToList() ?? new List<string>()));
            string done = string.Join(",", (Done?.Select(x => x.UsernameOrName()).ToList() ?? new List<string>()));
            return $"{UniqueID};{PublicID};{Raid?.AllValuesAsString()};{participants};{rejected};{done};{maybe};{IsPublished};{LastModificationTime}";
        }
    }
}
