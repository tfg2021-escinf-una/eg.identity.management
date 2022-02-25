namespace EG.IdentityManagement.Microservice.Settings
{
    public class MongoDbSettings
    {
        public string DatabaseName { set; get; }
        public string User { set; get; }
        public string Password { set; get; }

        public string ConnectionString =>
            $"mongodb+srv://{User}:{Password}@westazegclust01.omrzb.mongodb.net/{DatabaseName}?retryWrites=true&w=majority";
    }
}