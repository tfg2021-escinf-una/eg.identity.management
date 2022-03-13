using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Settings
{
    [ExcludeFromCodeCoverage]
    public class JwtSettings
    {
        public string ExpiresIn { set; get; }
        public string SecretKey { set; get; }
        public string Issuer { set; get; }
        public string Audience { set; get; }
        public string RefreshTokenExpiresIn { set; get; }
    }
}