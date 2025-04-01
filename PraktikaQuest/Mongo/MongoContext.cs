using MongoDB.Driver;

namespace Messenger.Mongo
{
    public class MongoContext
    {
        internal readonly IMongoDatabase database;

        internal readonly MongoClient mongoClient;

        public MongoContext(IConfiguration configuration)
        {
            string connectionString = configuration["DataBaseConnection"] ?? throw new ArgumentNullException("DataBaseConnection");
            string name = configuration["DataBaseName"] ?? throw new ArgumentException("DataBaseName");
            mongoClient = new MongoClient(connectionString);
            database = mongoClient.GetDatabase(name);
        }

        public MongoContext(string DataBaseConnection, string DataBaseName)
        {
            if (string.IsNullOrEmpty(DataBaseConnection))
            {
                throw new ArgumentNullException("DataBaseConnection");
            }

            if (string.IsNullOrEmpty(DataBaseName))
            {
                throw new ArgumentNullException("DataBaseName");
            }

            mongoClient = new MongoClient(DataBaseConnection);
            database = mongoClient.GetDatabase(DataBaseName);
        }
    }


    public static class MongoContextExtention
    {
        public static IServiceCollection AddMongoContext(this IServiceCollection services)
        {
            services.AddSingleton<MongoContext>();

            services//.AddCollectionContext<EventAction>("EventActions")
                    .AddCollectionContext<Inventory>("Inventory")
                    .AddCollectionContext<Item>("Items")
                    .AddCollectionContext<KeyEvent>("KeyEvents")
                    .AddCollectionContext<SceneBlock>("SceneBlocks")
                    //.AddCollectionContext<SceneRoute>("SceneRoutes")
                    .AddCollectionContext<GameState>("GameStates")
                    .AddCollectionContext<User>("Users");

            return services;
        }
    }

}
