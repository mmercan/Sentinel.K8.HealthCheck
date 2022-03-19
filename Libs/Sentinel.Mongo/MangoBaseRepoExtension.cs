using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sentinel.Mongo;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoBaseRepoExtension
    {


        public static IServiceCollection AddMongoRepo<T>(
               this IServiceCollection serviceCollection,
               MongoBaseRepoSettings<T> options) where T : new()
        {
            serviceCollection.Configure<MongoBaseRepoSettings<T>>(o => o = options);
            serviceCollection.AddSingleton<MongoBaseRepo<T>>();
            return serviceCollection;
        }

        public static IServiceCollection AddMongoRepo<T>(
        this IServiceCollection serviceCollection,
        IConfiguration options,
        string collectionName) where T : new()
        {
            // options["CollectionName"] = collectionName;
            // serviceCollection.Configure<MongoBaseRepoSettings<T>>(options);
            // serviceCollection.AddSingleton<MongoBaseRepo<T>>();
            // return serviceCollection;
            serviceCollection.AddSingleton<MongoBaseRepo<T>>((ctx) =>
            {
                //  var repo = sp.GetService<IDbRepository>();
                //     var apiKey = repo.GetApiKeyMethodHere();
                //     return new GlobalRepository(mode, apiKey);
                var logger = ctx.GetService<ILogger<MongoBaseRepo<T>>>();
                return new MongoBaseRepo<T>(options["ConnectionString"], options["DatabaseName"], collectionName, logger);
            });

            return serviceCollection;

        }

        public static IServiceCollection AddMongoRepo<T>(
        this IServiceCollection serviceCollection,
        Configuration.IConfiguration options) where T : new()
        {
            serviceCollection.Configure<MongoBaseRepoSettings<T>>(options);
            serviceCollection.AddSingleton<MongoBaseRepo<T>>();
            return serviceCollection;
        }



        public static IServiceCollection AddMongoRepo<T>(
        this IServiceCollection serviceCollection) where T : new()
        {
            serviceCollection.AddSingleton<MongoBaseRepo<T>>();
            return serviceCollection;
        }



        public static IServiceCollection AddMongoRepo<T>(
            this IServiceCollection serviceCollection,
            string connectionString,
            string databaseName,
            string collectionName) where T : new()
        {
            // serviceCollection.Configure<MongoBaseRepoSettings<T>>(options);
            // serviceCollection.AddSingleton<MongoBaseRepo<T>>();
            serviceCollection.AddSingleton<MongoBaseRepo<T>>((ctx) =>
            {
                //  var repo = sp.GetService<IDbRepository>();
                //     var apiKey = repo.GetApiKeyMethodHere();
                //     return new GlobalRepository(mode, apiKey);
                var logger = ctx.GetService<ILogger<MongoBaseRepo<T>>>();
                return new MongoBaseRepo<T>(connectionString, databaseName, collectionName, logger);
                // return RabbitHutch.CreateBus(Configuration["RabbitMQConnection"]);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddMongoRepo<T>(
             this IServiceCollection serviceCollection,
            string connectionString,
            string databaseName,
            string collectionName,
            Expression<Func<T, object>> IdField
            ) where T : new()
        {
            serviceCollection.AddSingleton<MongoBaseRepo<T>>((ctx) =>
            {
                var logger = ctx.GetService<ILogger<MongoBaseRepo<T>>>();
                logger.LogInformation("databasename : " + databaseName + " collectionName : " + collectionName);
                return new MongoBaseRepo<T>(connectionString, databaseName, collectionName, IdField, logger);
            });
            return serviceCollection;
        }

    }
}