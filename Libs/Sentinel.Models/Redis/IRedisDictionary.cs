using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sentinel.Models.Redis
{
    public interface IRedisDictionary<TValue> : IDictionary<string, TValue>
    {
        void Add(TValue value);

        void Add(string key, TValue value);
        Task AddAsync(TValue value);
        bool ContainsKey(TValue value);
        bool Remove(KeyValuePair<string, TValue> item);
        bool Remove(TValue value);
        bool Remove(string key);
        bool TryGetValue(string key, out TValue value);
        ICollection<TValue> Values { get; }
        ICollection<string> Keys { get; }
        void Add(KeyValuePair<string, TValue> item);
        void Clear();
        bool Contains(KeyValuePair<string, TValue> item);
        void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex);
        int Count { get; }
        bool IsReadOnly { get; }
        ILogger Logger { get; }
        IEnumerator<KeyValuePair<string, TValue>> GetEnumerator();
        void AddMultiple(IEnumerable<TValue> items);
        void AddMultiple(IEnumerable<KeyValuePair<string, TValue>> items);
        void Sync(IEnumerable<TValue> items);
        void Sync(IEnumerable<TValue> items, Func<TValue, string> keyFunc);


    }
}