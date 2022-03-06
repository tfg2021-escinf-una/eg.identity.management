using System;
using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Entities.Identity
{
    [ExcludeFromCodeCoverage]
    public class RefreshToken : IDisposable
    {
        public string JwtId { set; get; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string LoginProvider { get; set; }
        public DateTime IssuedAt { set; get; }
        public DateTime ExpiresAt { set; get; }
        public bool Used { set; get; }

        public void Dispose()
        { }
    }
}