using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Mongo
{
    public class MongoBaseRepo<T> where T : new()
    {
        public IMongoDatabase MongoDb { get; private set; }
        public MongoClient mongoClient { get; private set; }
        public string IdFieldName { get; private set; }
        private string collectionName;

        private string timestampFieldName;
        private string metaFieldName;
        readonly ILogger<MongoBaseRepo<T>> logger;
        private readonly bool isTimeSeries = false;
        public MongoBaseRepo(IOptions<MongoBaseRepoSettings<T>> options, ILogger<MongoBaseRepo<T>> logger, string collectionName) : this(options.Value.ConnectionString, options.Value.DatabaseName, collectionName, logger)
        {

        }

        public MongoBaseRepo(IOptions<MongoBaseRepoSettings<T>> options, ILogger<MongoBaseRepo<T>> logger) : this(options.Value.ConnectionString, options.Value.DatabaseName, options.Value.CollectionName, logger)
        {

        }

        public MongoBaseRepo(string connectionString, string databaseName, string collectionName, string IdField, ILogger<MongoBaseRepo<T>> logger)
        {
            this.logger = logger;
            init(connectionString, databaseName, collectionName, IdField);
        }

        public MongoBaseRepo(string connectionString, string databaseName, string collectionName, ILogger<MongoBaseRepo<T>> logger) : this(connectionString, databaseName, collectionName, null as Expression<Func<T, object>>, logger)
        {

        }

        public MongoBaseRepo(string connectionString, string databaseName, string collectionName, Expression<Func<T, object>> IdField, ILogger<MongoBaseRepo<T>> logger)
        {
            this.logger = logger;
            string? field = null;
            if (IdField?.Body != null && IdField.Body is MemberExpression)
            {
                field = (IdField.Body as MemberExpression).Member.Name;
            }
            this.init(connectionString, databaseName, collectionName, field);
        }

        public MongoBaseRepo(string connectionString, string databaseName, string collectionName,
        Expression<Func<T, object>> IdField,
        Expression<Func<T, object>> timestampFileld,
        Expression<Func<T, object>> metaFileld,
        ILogger<MongoBaseRepo<T>> logger)
        {
            this.logger = logger;
            string? field = null;
            if (IdField?.Body != null && IdField.Body is MemberExpression)
            {
                field = (IdField.Body as MemberExpression).Member.Name;
            }

            if (timestampFileld?.Body != null && timestampFileld.Body is MemberExpression)
            {
                timestampFieldName = (timestampFileld.Body as MemberExpression).Member.Name;
            }
            else if (timestampFileld?.Body != null && timestampFileld.Body is UnaryExpression)
            {
                timestampFieldName = ((timestampFileld.Body as UnaryExpression).Operand as MemberExpression)?.Member.Name;
            }


            if (metaFileld?.Body != null && metaFileld.Body is MemberExpression)
            {
                metaFieldName = (metaFileld.Body as MemberExpression).Member.Name;
            }
            this.isTimeSeries = true;

            this.init(connectionString, databaseName, collectionName, field);

            var check = CollectionExistsAsync(collectionName);
            Task.WaitAll(check);
            if (!check.Result)
            {
                CreateTimeSeriesCollection(collectionName, this.timestampFieldName, metaFieldName);
            }
        }

        private void init(string connectionString, string databaseName, string collectionName, string? IdField)
        {
            if (IdField != null)
            {
                this.IdFieldName = IdField;
                CreateIdMap();
            }
            else
            {
                if (BsonClassMap.IsClassMapRegistered(typeof(T)))
                {
                    this.IdFieldName = BsonClassMap.LookupClassMap(typeof(T))?.IdMemberMap?.MemberInfo?.Name;
                }
                if (IdFieldName == null)
                {
                    Type typeParameterType = typeof(T);
                    var item = typeParameterType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(p1 => p1.AttributeType == typeof(BsonIdAttribute)));
                    if (item == null)
                    {
                        item = typeParameterType.GetProperties().FirstOrDefault(p => p.CustomAttributes.Any(p1 => p1.AttributeType == typeof(KeyAttribute)));
                        if (item != null)
                        {
                            this.IdFieldName = item.Name;
                            CreateIdMap();
                        }
                    }
                    if (item == null)
                    {
                        item = typeParameterType.GetProperties().FirstOrDefault(p => p.Name.ToLower() == "id");
                    }
                    if (item != null)
                    {
                        this.IdFieldName = item.Name;
                    }
                }
            }

            logger.LogCritical(" IdFieldName " + IdFieldName);
            logger.LogCritical(" collectionName " + collectionName);

            this.collectionName = collectionName;
            mongoClient = new MongoClient(connectionString);
            MongoDb = mongoClient.GetDatabase(databaseName);
        }
        public IMongoCollection<T> Items
        {
            get
            {
                var check = CollectionExistsAsync(collectionName);
                Task.WaitAll(check);
                if (!check.Result)
                {
                    if (isTimeSeries)
                    {
                        CreateTimeSeriesCollection(collectionName, timestampFieldName, metaFieldName);
                    }
                    else
                    {
                        MongoDb.CreateCollection(collectionName);
                    }
                    var collection = MongoDb.GetCollection<T>(collectionName);
                    InitialDatabase(collection);
                    return collection;
                }
                return MongoDb.GetCollection<T>(collectionName);
            }
        }

        private IMongoCollection<T>? CreateTimeSeriesCollection(string collectionName, string timestamp, string? metaField)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            MongoDb.CreateCollection(collectionName,
                new CreateCollectionOptions { TimeSeriesOptions = new TimeSeriesOptions(timestamp, metaField) });
            var collection = MongoDb.GetCollection<T>(collectionName);
            return collection;
        }
        public IEnumerable<T> GetAll()
        {
            var name = typeof(T).ToString();
            logger.LogCritical(name + " collectionName " + collectionName);
            return Items.Find(FilterDefinition<T>.Empty).ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var name = typeof(T).ToString();
            logger.LogCritical(name + " collectionName " + collectionName);
            var res = await Items.FindAsync(FilterDefinition<T>.Empty);
            return res.ToList();
        }

        public IEnumerable<T> Find(FilterDefinition<T> filter)
        {
            var name = typeof(T).ToString();
            return Items.Find(filter).ToList();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> filter)
        {
            var name = typeof(T).ToString();
            return Items.Find(filter).ToList();
        }


        public async Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter)
        {
            var name = typeof(T).ToString();
            var res = await Items.FindAsync(filter);
            return res.ToList();
        }


        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            var name = typeof(T).ToString();
            var res = await Items.FindAsync(filter);
            return res.ToList();
        }

        public async Task<T> Get(BsonDocument filter)
        {
            return await Items.FindSync<T>(filter).FirstOrDefaultAsync();
        }
        public async Task<T> Get(FilterDefinition<T> filter)
        {
            return await Items.FindSync<T>(filter).FirstOrDefaultAsync();
        }
        public async Task<T> GetByIDAsync(object value)
        {
            if (IdFieldName != null)
            {
                var filter = Builders<T>.Filter.Eq(IdFieldName, value);
                return await Get(filter);
            }
            else
            {
                throw new MissingFieldException("ID field not Defined, use Key or BsonId Attribute in your Model or define on the constructor");
            }
        }
        public async Task AddAsync(T item)
        {
            await Items.InsertOneAsync(item);
        }
        public async Task AddAsync(IEnumerable<T> items)
        {
            await Items.InsertManyAsync(items);
        }
        public async Task<ReplaceOneResult> UpdateAsync(object id, T item)
        {
            var filter = Builders<T>.Filter.Eq(IdFieldName, id);
            var result = await Items.ReplaceOneAsync(filter, item);
            return result;
        }
        public async Task<ReplaceOneResult> UpdateAsync(T item)
        {
            if (IdFieldName != null)
            {
                var id = item.GetType().GetRuntimeProperty(IdFieldName).GetValue(item);
                var filter = Builders<T>.Filter.Eq(IdFieldName, id);
                var result = await Items.ReplaceOneAsync(filter, item);
                return result;
            }
            else
            {
                throw new MissingFieldException("ID field not Defined, use Key or BsonId Attribute in your Model or define on the constructor");
            }
        }
        public async Task RemoveAsync(object id)
        {
            if (IdFieldName == null) { throw new MissingFieldException("ID field not Defined, use Key or BsonId Attribute in your Model or define on the constructor"); }
            if (id == null) { throw new ArgumentNullException("ID Field can not be Null"); }
            var filter = Builders<T>.Filter.Eq(IdFieldName, id);
            await Items.DeleteOneAsync(filter);
        }
        public async Task RemoveAsync(T item)
        {
            if (IdFieldName == null) { throw new MissingFieldException("ID field not Defined, use Key or BsonId Attribute in your Model or define on the constructor"); }
            if (item == null) { throw new ArgumentNullException("item can not be Null"); }

            var id = item.GetType().GetRuntimeProperty(IdFieldName).GetValue(item);
            var filter = Builders<T>.Filter.Eq(IdFieldName, id);
            await Items.DeleteOneAsync(filter);
        }

        public async Task Upsert(T item, Expression<Func<T, bool>> filter)
        {
            await Items.ReplaceOneAsync(
                filter: filter,
                replacement: item,
                options: new UpdateOptions { IsUpsert = true }
             );
        }

        public async Task<T> GetAsync<TProperty>(Expression<Func<T, TProperty>> property, object value)
        {
            if (property.Body is System.Linq.Expressions.MemberExpression)
            {
                var pro = (property.Body as MemberExpression).Member;
                var propertyName = nameof(pro.Name);

                var filter = Builders<T>.Filter.Eq(propertyName, value);
                return await Get(filter);
            }
            else { return default(T); }
        }
        public async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await MongoDb.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return (await collections.ToListAsync()).Any();
        }
        public async Task DropCollectionAsync(string collectionName)
        {
            await MongoDb.DropCollectionAsync(collectionName);
        }
        public void DropCollection(string collectionName)
        {
            MongoDb.DropCollection(collectionName);
        }
        private void CreateIdMap()
        {
            if (IdFieldName != null)
            {
                if (BsonClassMap.IsClassMapRegistered(typeof(T)))
                {
                    try
                    {
                        BsonClassMap.LookupClassMap(typeof(T)).AutoMap();
                        BsonClassMap.LookupClassMap(typeof(T)).MapIdProperty(IdFieldName);
                    }
                    catch
                    {
                        logger.LogError("Error in CreateIdMap");
                    }
                }
                else
                {
                    BsonClassMap.RegisterClassMap<T>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdProperty(IdFieldName);
                    });
                }
            }
        }
        public virtual void InitialDatabase(IMongoCollection<T> collection)
        {

        }
    }
    public class Gen<T> : IIdGenerator
    {
        public object GenerateId(object container, object document)
        {
            if (container is MongoDB.Driver.IMongoCollection<T>)
            {
                // (container as MongoDB.Driver.IMongoCollection<T>).Find(null).Max
            }
            throw new NotImplementedException();
        }

        public bool IsEmpty(object id)
        {
            throw new NotImplementedException();
        }
    }

    public class MongoBaseRepoSettings<T> where T : new()
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string CollectionName { get; set; } = default!;
        public string IdField { get; set; } = default!;
    }
}