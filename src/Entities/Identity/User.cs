using AspNetCore.Identity.Mongo.Model;
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

        public List<string> JwtTokens { set; get; }
        public List<string> RefreshTokens { set; get; }

        public User()
        {
            JwtTokens = new List<string>();
            RefreshTokens = new List<string>();
        }
    }
}