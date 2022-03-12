using AspNetCore.Identity.Mongo.Model;
using EG.IdentityManagement.Microservice.Entities.Identity;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EG.IdentityManagement.Microservice.Identity
{
    public class User : MongoUser<string>, IDisposable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [BsonIgnore]
        public string Password { set; get; }

        public JwtToken JwtToken { set; get; }
        public RefreshToken RefreshToken { set; get; }

        public User()
        {
            JwtToken = null;
            RefreshToken = null;
        }

        public void Dispose()
        { }
    }
}