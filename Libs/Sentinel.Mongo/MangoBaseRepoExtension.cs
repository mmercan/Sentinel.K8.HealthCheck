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
    public static class MangoBaseRepoExtension
    {


        public static IServiceCollection AddMangoRepo<T>(
               this IServiceCollection serviceCollection,
               MangoBaseRepoSettings<T> options) where T : new()
        {
            serviceCollection.Configure<MangoBaseRepoSettings<T>>(o => o = options);
            serviceCollection.AddSingleton<MangoBaseRepo<T>>();
            return serviceCollection;
        }

        public static IServiceCollection AddMangoRepo<T>(
        this IServiceCollection serviceCollection,
        IConfiguration options,
        string collectionName) where T : new()
        {
            // options["CollectionName"] = collectionName;
            // serviceCollection.Configure<MangoBaseRepoSettings<T>>(options);
            // serviceCollection.AddSingleton<MangoBaseRepo<T>>();
            // return serviceCollection;
            serviceCollection.AddSingleton<MangoBaseRepo<T>>((ctx) =>
            {
                //  var repo = sp.GetService<IDbRepository>();
                //     var apiKey = repo.GetApiKeyMethodHere();
                //     return new GlobalRepository(mode, apiKey);
                var logger = ctx.GetService<ILogger<MangoBaseRepo<T>>>();
                return new MangoBaseRepo<T>(options["ConnectionString"], options["DatabaseName"], collectionName, logger);
            });

            return serviceCollection;

        }

        public static IServiceCollection AddMangoRepo<T>(
        this IServiceCollection serviceCollection,
        Configuration.IConfiguration options) where T : new()
        {
            serviceCollection.Configure<MangoBaseRepoSettings<T>>(options);
            serviceCollection.AddSingleton<MangoBaseRepo<T>>();
            return serviceCollection;
        }



        public static IServiceCollection AddMangoRepo<T>(
        this IServiceCollection serviceCollection) where T : new()
        {
            serviceCollection.AddSingleton<MangoBaseRepo<T>>();
            return serviceCollection;
        }



        public static IServiceCollection AddMangoRepo<T>(
            this IServiceCollection serviceCollection,
            string connectionString,
            string databaseName,
            string collectionName) where T : new()
        {
            // serviceCollection.Configure<MangoBaseRepoSettings<T>>(options);
            // serviceCollection.AddSingleton<MangoBaseRepo<T>>();
            serviceCollection.AddSingleton<MangoBaseRepo<T>>((ctx) =>
            {
                //  var repo = sp.GetService<IDbRepository>();
                //     var apiKey = repo.GetApiKeyMethodHere();
                //     return new GlobalRepository(mode, apiKey);
                var logger = ctx.GetService<ILogger<MangoBaseRepo<T>>>();
                return new MangoBaseRepo<T>(connectionString, databaseName, collectionName, logger);
                // return RabbitHutch.CreateBus(Configuration["RabbitMQConnection"]);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddMangoRepo<T>(
             this IServiceCollection serviceCollection,
            string connectionString,
            string databaseName,
            string collectionName,
            Expression<Func<T, object>> IdField
            ) where T : new()
        {
            serviceCollection.AddSingleton<MangoBaseRepo<T>>((ctx) =>
            {
                //  var repo = sp.GetService<IDbRepository>();
                //     var apiKey = repo.GetApiKeyMethodHere();
                //     return new GlobalRepository(mode, apiKey);
                var logger = ctx.GetService<ILogger<MangoBaseRepo<T>>>();
                logger.LogInformation("databasename : " + databaseName + " collectionName : " + collectionName);
                return new MangoBaseRepo<T>(connectionString, databaseName, collectionName, IdField, logger);
                // return RabbitHutch.CreateBus(Configuration["RabbitMQConnection"]);
            });

            return serviceCollection;
        }

    }
}