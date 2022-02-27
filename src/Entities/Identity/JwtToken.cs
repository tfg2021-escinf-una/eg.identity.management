using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EG.IdentityManagement.Microservice.Entities.Identity
{
    public class JwtToken : IdentityUserToken<string>
    {
        [BsonId]
        public string Id { set; get; }

        public string JwtId { set; get; }
        public DateTime IssuedAt { set; get; }
        public DateTime ExpiresAt { set; get; }
    }
}