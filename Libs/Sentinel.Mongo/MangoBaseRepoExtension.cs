using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            serviceCollection.AddSingleton<MongoBaseRepo<T>>((ctx) =>
            {
                var logger = ctx.GetService<ILogger<MongoBaseRepo<T>>>();
                if (logger == null)
                {
                    //TODO :should throw exception or use NullLogger when logger is null
                    logger = new NullLogger<MongoBaseRepo<T>>();
                }
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
            serviceCollection.AddSingleton<MongoBaseRepo<T>>((ctx) =>
            {
                var logger = ctx.GetService<ILogger<MongoBaseRepo<T>>>();
                if (logger == null)
                {
                    //TODO :should throw exception or use NullLogger when logger is null
                    logger = new NullLogger<MongoBaseRepo<T>>();
                }
                return new MongoBaseRepo<T>(connectionString, databaseName, collectionName, logger);
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
                if (logger == null)
                {
                    //TODO :should throw exception or use NullLogger when logger is null
                    logger = new NullLogger<MongoBaseRepo<T>>();
                }
                logger.LogInformation("databasename : " + databaseName + " collectionName : " + collectionName);
                return new MongoBaseRepo<T>(connectionString, databaseName, collectionName, IdField, logger);
            });
            return serviceCollection;
        }


        public static IServiceCollection AddMongoTimeSeriesRepo<T>(
         this IServiceCollection serviceCollection,
        string connectionString,
        string databaseName,
        string collectionName,
        Expression<Func<T, object>> IdField,
        Expression<Func<T, object>> timestampFileld,
        Expression<Func<T, object>> metaFileld
        ) where T : new()
        {
            serviceCollection.AddSingleton<MongoBaseRepo<T>>((ctx) =>
            {
                var logger = ctx.GetService<ILogger<MongoBaseRepo<T>>>();
                if (logger == null)
                {
                    //TODO :should throw exception or use NullLogger when logger is null ??
                    logger = new NullLogger<MongoBaseRepo<T>>();
                }
                logger.LogInformation("databasename : " + databaseName + " collectionName : " + collectionName);
                return new MongoBaseRepo<T>(connectionString, databaseName, collectionName, IdField, timestampFileld, metaFileld, logger);
            });
            return serviceCollection;
        }

    }
}