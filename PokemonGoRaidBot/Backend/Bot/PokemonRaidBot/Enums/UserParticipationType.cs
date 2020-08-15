namespace RaidBot.Backend.Bot.PokemonRaidBot.Enums
{
    /// <summary>
    /// Defines the available user participation types.
    /// </summary>
    public enum UserParticipationType
    {
        /// User will be physically coming to the gym for the raid.
        InRealLife = 0,

        /// User is going to participate remotely.
        Remote = 1
    }
}