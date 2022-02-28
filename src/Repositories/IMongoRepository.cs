using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Repositories
{
    public interface IMongoRepository<T>
    {
        Task<bool> InsertOneAsync(T obj);

        Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default);

        T Find(FilterDefinition<T> filter, FindOptions options = null);

        Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);

        Task<TNewResult> FindOneLookupAsync<TResult, TForeignDocument, TNewResult>(string foreignCollection, FieldDefinition<T> localField, FieldDefinition<TForeignDocument> foreignField, FieldDefinition<TNewResult> @as, FilterDefinition<T> filter);

        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> filter, FindOptions options = null);
    }
}