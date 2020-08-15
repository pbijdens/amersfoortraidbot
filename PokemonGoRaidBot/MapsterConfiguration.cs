using Mapster;
using Microsoft.AspNetCore.Builder;
using RaidBot.Backend.API;
using RaidBot.Backend.Bot.PokemonRaidBot.Entities;
using RaidBot.Backend.Bot.PokemonRaidBot.Enums;
using System.Collections.Generic;
using System.Linq;

namespace RaidBot
{
    // https://github.com/MapsterMapper/Mapster/wiki/Configuration
    // https://github.com/MapsterMapper/Mapster/wiki/Custom-mapping

    public static class MapsterConfiguration
    {
        // We user Mapster to convert Database objects to API objects automatically using the .Adapt<T>() operator
        // This file is the sole location where we set up Mapster configurations for types.

        public static void UseMapster(this IApplicationBuilder app)
        {
            TypeAdapterConfig<RaidParticipation, McRaidDescription>
                .ForType()
                .Map(dest => dest.Raid, src => src.Raid.Raid)
                .Map(dest => dest.Gym, src => src.Raid.Gym)
                .Map(dest => dest.Location, src => src.Raid.Location.Adapt<McLocation>())
                .Map(dest => dest.RaidUnlockTime, src => src.Raid.RaidUnlockTime)
                .Map(dest => dest.RaidEndTime, src => src.Raid.RaidEndTime)
                .Map(dest => dest.Address, src => src.Raid.Address)
                .Map(dest => dest.Alignment, src => src.Raid.Alignment)
                .Map(dest => dest.Remarks, src => src.Raid.Remarks)
                .Map(dest => dest.UniqueID, src => src.UniqueID)
                .Map(dest => dest.PublicID, src => src.PublicID)
                .Map(dest => dest.UpdateCount, src => src.Raid.UpdateCount)
                .Map(dest => dest.User, src => src.Raid.User.Adapt<McTelegramUser>())
                .Map(dest => dest.Valor, src => src.Participants.Get(Team.Valor).Select(x => 1 + x.Extra).Sum())
                .Map(dest => dest.Mystic, src => src.Participants.Get(Team.Mystic).Select(x => 1 + x.Extra).Sum())
                .Map(dest => dest.Instinct, src => src.Participants.Get(Team.Instinct).Select(x => 1 + x.Extra).Sum())
                .Map(dest => dest.Unknown, src => src.Participants.Get(Team.Unknown).Select(x => 1 + x.Extra).Sum())
                .Map(dest => dest.Maybe, src => src.Maybe != null ? src.Maybe.Count : 0);
        }
    }

    public static class Extensions
    {
        public static List<UserParticipation> Get(this Dictionary<Team, List<UserParticipation>> ths, Team team)
        {
            return ths.ContainsKey(team) ? (ths[team] ?? new List<UserParticipation>()) : new List<UserParticipation>();
        }
    }
}
