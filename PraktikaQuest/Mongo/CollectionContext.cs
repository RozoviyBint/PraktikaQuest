using MongoDB.Bson;
using MongoDB.Driver;
using System.Xml.Linq;

namespace Messenger.Mongo
{
    public class CollectionContext<T>(MongoContext mongoContext, string name) where T : class
    {
        public IMongoCollection<T> collection = mongoContext.database.GetCollection<T>(name);


        public async Task<List<T>> FindDocs(FilterDefinition<T> filter)
        {
            return await collection.Find<T>(filter).ToListAsync();
        }
        public async Task<List<T>> FindAllAsync()
        {
            return await collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        }

        public async Task<T> FindFirstAsync(BsonValue id)
        {
            return await collection.Find((FilterDefinition<T>)new BsonDocument { { "_id", id } }).FirstOrDefaultAsync();
        }
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return await collection.UpdateOneAsync(filter, update);
        }

        public async Task<T> FindByAnyFieldAsync(string fieldName, BsonValue field)
        {
            return await collection.Find(Builders<T>.Filter.Eq(fieldName, field)).FirstOrDefaultAsync();
        }

        public async Task<T> UpdateDocument(BsonValue id, UpdateDefinition<T> updateDefinition)
        {
            return await collection.FindOneAndUpdateAsync(new BsonDocument { { "_id", id } }, updateDefinition, new FindOneAndUpdateOptions<T>
            {
                ReturnDocument = ReturnDocument.After
            });
        }

        public async Task<bool> DeleteDocument(BsonValue id)
        {
            return (await collection.DeleteOneAsync(new BsonDocument { { "_id", id } })).DeletedCount > 0;
        }

        public async Task<IEnumerable<T>> GetDocsThroughPagination(int page, int quantity)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            return await collection.Find(Builders<T>.Filter.Empty).Skip((page - 1) * quantity).Limit(quantity)
                .ToListAsync();
        }

        public async Task InsertDocument(T value)
        {
            try
            {
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    // Строим поле для сортировки "_id" или "Id"
                    var sortField = idProperty.Name;

                    // Получаем максимальный Id без dynamic
                    var sort = Builders<T>.Sort.Descending(sortField);
                    var lastDoc = await collection.Find(Builders<T>.Filter.Empty)
                                                  .Sort(sort)
                                                  .Limit(1)
                                                  .FirstOrDefaultAsync();

                    int nextId = 1;
                    if (lastDoc != null)
                    {
                        var lastIdValue = idProperty.GetValue(lastDoc);
                        nextId = (int)lastIdValue + 1;
                    }

                    idProperty.SetValue(value, nextId);
                }

                await collection.InsertOneAsync(value);
            }
            catch (MongoWriteException ex)
            {
                // Можно сюда добавить логирование
                Console.WriteLine($"Ошибка вставки: {ex.Message}");
            }
        }
    }


    public static class CollectionContextExtention
    {
        public static IServiceCollection AddCollectionContext<T>(this IServiceCollection services, string collectionName) where T : class
        {
            services.AddSingleton((IServiceProvider options) => new CollectionContext<T>(options.GetRequiredService<MongoContext>(), collectionName));
            return services;
        }
    }
}
