namespace EG.IdentityManagement.Microservice.Settings
{
    public class JwtSettings
    {
        public int ExpiresIn { set; get; }
        public string SecretKey { set; get; }
        public string Issuer { set; get; }
        public string Audience { set; get; }
        public int RefreshTokenExpiresIn { set; get; }
    }
}