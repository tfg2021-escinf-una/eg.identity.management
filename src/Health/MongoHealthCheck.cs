using EG.IdentityManagement.Microservice.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Health
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly MongoDbSettings _mongoDbSettings;

        public MongoHealthCheck(IOptions<MongoDbSettings> mongoSettings)
        {
            _mongoDbSettings = mongoSettings?.Value ?? throw new ArgumentNullException("Values should not come null");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var mongoClient = new MongoClient(_mongoDbSettings.ConnectionString());
                var db = mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
                await db.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            }
            catch(Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Service unavailable", ex);
            }

            return new HealthCheckResult(HealthStatus.Healthy, "Service available");
        }
    }
}
