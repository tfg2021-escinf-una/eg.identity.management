using System;

namespace EG.IdentityManagement.Microservice.Entities.Identity
{
    public class JwtToken : IDisposable
    {
        public string Id { set; get; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime IssuedAt { set; get; }
        public DateTime ExpiresAt { set; get; }

        public void Dispose()
        {
        }
    }
}