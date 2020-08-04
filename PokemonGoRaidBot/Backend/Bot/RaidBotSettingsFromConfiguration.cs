using RaidBot.Backend.Bot.PokemonRaidBot;
using System.Collections.Generic;

namespace RaidBot.Backend.Bot
{
    public class RaidBotSettings : ISettingsManager
    {
        public string Name { get; set; }

        public string DataFolder { get; set; }

        public string BotAddress { get; set; }

        public string BotKey { get; set; }

        public string[] Timezones { get; set; }

        public long? PublicationChannel { get; set; }

        public string GoogleLocationAPIKey { get; set; }

        public string Language { get; set; }

        public List<PogoAfoMapping> PogoAfoMappings { get; set; }
    }
}
