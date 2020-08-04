using Newtonsoft.Json;

namespace RaidBot.Backend.API
{
    [JsonObject(MemberSerialization.OptOut)]
    public class McLocation
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

    }
}
