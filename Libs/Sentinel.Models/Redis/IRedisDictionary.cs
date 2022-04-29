using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sentinel.Models.Redis
{
    public interface IRedisDictionary<TValue> : IDictionary<string, TValue>
    {
        bool ContainsKey(TValue value);
        bool Remove(TValue value);
        ILogger Logger { get; }
        void Add(TValue value);
        Task AddAsync(TValue value);
        void AddMultiple(IEnumerable<TValue> items);
        void AddMultiple(IEnumerable<KeyValuePair<string, TValue>> items);
        void Sync(IEnumerable<TValue> items, bool overrideExisting = true);
        void Sync(IEnumerable<TValue> items, Func<TValue, string> keyFunc);

    }
}