using AspNetCore.Identity.Mongo.Model;
using EG.IdentityManagement.Microservice.Entities.Identity;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace EG.IdentityManagement.Microservice.Identity
{
    public class User : MongoUser<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [BsonIgnore]
        public string Password { set; get; }

        public List<JwtToken> JwtTokens { get; }
        public List<RefreshToken> RefreshTokens { set; get; }

        public User()
        {
            JwtTokens = new List<JwtToken>();
            RefreshTokens = new List<RefreshToken>();
        }
    }
}