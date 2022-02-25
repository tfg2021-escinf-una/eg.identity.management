namespace EG.IdentityManagement.Microservice.Entities.Const
{
    public static class Constants
    {
        public enum ApplicationRoles
        {
            Administrator,
            Moderator,
            StandardUser
        }

        public enum TokenPurpose
        {
            JWT,
            RefreshToken
        }

        public const string AuthTokenProvider = "AuthTokenProvider";
        public const string AccessToken = "AccessToken";
        public const string ExpiresOn = "ExpiresOn";
        public const string RefreshToken = "RefreshToken";
        public const string UserName = "username";
    }
}