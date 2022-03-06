using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Settings
{
    [ExcludeFromCodeCoverage]
    public class MongoDbSettings
    {
        public string DatabaseName { set; get; }
        public string User { set; get; }
        public string Password { set; get; }
        public bool IsAppUsingLocalDb { set; get; } = false;

        public string ConnectionString() =>
           !IsAppUsingLocalDb ? $"mongodb+srv://{User}:{Password}@westazegclust01.omrzb.mongodb.net/{DatabaseName}?retryWrites=true&w=majority"
                              : $"mongodb://127.0.0.1:27017/{DatabaseName}";
    }
}