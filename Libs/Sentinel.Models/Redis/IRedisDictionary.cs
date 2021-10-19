using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sentinel.Models.Redis
{
    public interface IRedisDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        void Add(TValue value);

        void Add(TKey key, TValue value);
        void Add(string key, TValue value);
        Task AddAsync(TValue value);
        bool ContainsKey(TValue value);
        bool Remove(KeyValuePair<TKey, TValue> item);
        bool Remove(TValue value);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        ICollection<TValue> Values { get; }
        ICollection<TKey> Keys { get; }
        void Add(KeyValuePair<TKey, TValue> item);
        void Clear();
        bool Contains(KeyValuePair<TKey, TValue> item);
        void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex);
        int Count { get; }
        bool IsReadOnly { get; }
        ILogger Logger { get; }
        IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        void AddMultiple(IEnumerable<TValue> items);
        void AddMultiple(IEnumerable<KeyValuePair<TKey, TValue>> items);
        void Sync(IEnumerable<TValue> items);
        void Sync(IEnumerable<TValue> items, Func<TValue, string> keyFunc);


    }
}