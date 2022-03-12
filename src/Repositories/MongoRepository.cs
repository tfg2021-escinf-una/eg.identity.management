using EG.IdentityManagement.Microservice.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Repositories
{
    [ExcludeFromCodeCoverage]
    public class MongoRepository<T> : IMongoRepository<T>
    {
        private readonly MongoDbSettings _mongoDbSettings;
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<MongoRepository<T>> _logger;
        private readonly string _collectionName;

        public MongoRepository(IOptions<MongoDbSettings> mongoDbSettings,
                               ILogger<MongoRepository<T>> logger,
                               IMongoClient mongoClient)
        {
            _mongoDbSettings = mongoDbSettings.Value ?? throw new ArgumentNullException("Value should not come as null");
            _mongoClient = mongoClient ?? throw new ArgumentNullException("Value should not come as null");
            _logger = logger;
            _collectionName = $"{typeof(T).Name}s";
        }

        public async Task<bool> InsertOneAsync(T obj)
        {
            try
            {
                var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
                var collection = database.GetCollection<T>(_collectionName);
                await collection.InsertOneAsync(obj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unhandled error has occurred while inserting a document in the collection: {_collectionName}");
                return false;
            }

            return true;
        }

        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter,
                                                       CancellationToken cancellationToken = default)
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
            var collection = database.GetCollection<T>(_collectionName);
            return await collection.DeleteOneAsync(filter, cancellationToken);
        }

        public T Find(FilterDefinition<T> filter, FindOptions options = null)
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
            var collection = database.GetCollection<T>(_collectionName);
            return collection.Find(filter, options)
                    .FirstOrDefault();
        }

        public async Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter,
                                                   UpdateDefinition<T> update,
                                                   CancellationToken cancellationToken = default)
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
            var collection = database.GetCollection<T>(_collectionName);
            return await collection.FindOneAndUpdateAsync(
                filter,
                update,
                options: new FindOneAndUpdateOptions<T>
                {
                    // Do this to get the record AFTER the updates are applied
                    ReturnDocument = ReturnDocument.After
                });
        }

        public async Task<TNewResult> FindOneLookupAsync<TResult, TForeignDocument, TNewResult>(
                                          string foreignCollection,
                                          FieldDefinition<T> localField,
                                          FieldDefinition<TForeignDocument> foreignField,
                                          FieldDefinition<TNewResult> @as,
                                          FilterDefinition<T> filter)
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
            var collection = database.GetCollection<T>(_collectionName);
            return await collection
                       .Aggregate()
                       .Match(filter)
                       .Lookup(foreignCollection,
                              localField,
                              foreignField,
                              @as)
                      .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> filter, FindOptions options = null)
        {
            var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
            var collection = database.GetCollection<T>(_collectionName);
            return await collection.Find(filter, options)
                    .ToListAsync();
        }
    }
}