namespace RaidBot.Backend.API
{
    public class McTelegramUser
    {
        public long ID { get; set; }
        public bool IsBot { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string LanguageCode { get; set; }
    }
}
