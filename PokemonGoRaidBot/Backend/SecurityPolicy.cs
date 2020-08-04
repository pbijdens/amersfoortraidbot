namespace RaidBot.Backend
{
    public static class SecurityPolicy
    {
        public const string RoleAdministrator = "Administrator";

        public static readonly string[] AllRoles = new string[] {
            RoleAdministrator,
        };

        public const string IsAdministrator = "CanManageUsers";
    }
}
