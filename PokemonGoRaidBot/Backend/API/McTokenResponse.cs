using Newtonsoft.Json;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McTokenResponse
    {
        public string token { get; set; }
        public int expiration { get; set; }
        public string refresh_token { get; set; }
    }
}
