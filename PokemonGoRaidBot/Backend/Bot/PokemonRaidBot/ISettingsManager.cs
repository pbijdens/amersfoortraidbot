using Botje.Messaging.Models;
using System.Collections.Generic;

namespace RaidBot.Backend.Bot.PokemonRaidBot
{
    public class PogoAfoMapping
    {
        public string Url { get; set; }
        public long Channel { get; set; }
        public List<RaidTarget> Targets { get; set; }
    }

    public class RaidTarget
    {
        public string Description { get; set; }

        public long ChannelID { get; set; }

        public List<int> Levels { get; set; }

        public List<int> ExRaidLevels { get; set; }

        public Location NorthEastCorner { get; set; }

        public Location SouthWestCorner { get; set; }
    }

    /// <summary>
    /// Settings manager.
    /// </summary>
    public interface ISettingsManager
    {
        string DataFolder { get; }
        string BotKey { get; }
        string[] Timezones { get; }
        long? PublicationChannel { get; }
        string GoogleLocationAPIKey { get; }
        string Language { get; } // valid culture
        List<PogoAfoMapping> PogoAfoMappings { get; }
        string Name { get; }
        string BotAddress { get; }
    }
}
