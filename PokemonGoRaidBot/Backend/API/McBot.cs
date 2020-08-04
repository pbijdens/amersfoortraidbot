using Newtonsoft.Json;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McBot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsStarted { get; set; }
    }
}
