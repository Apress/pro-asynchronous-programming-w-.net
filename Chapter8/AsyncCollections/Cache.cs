using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AsyncCollections
{
    public class Cache<K,V>
    {
        private readonly Func<K, Task<V>> calculateValue;
        ConcurrentDictionary<K,Lazy<Task<V>>>  map = new ConcurrentDictionary<K, Lazy<Task<V>>>();

        public Cache(Func<K,Task<V>> calculateValue )
        {
            this.calculateValue = calculateValue;
        }

        public Task<V> this[K index]
        {
            get
            {
                var value = new Lazy<Task<V>>(() => calculateValue(index));

                return map.GetOrAdd(index, value).Value;
            }
        }
    }
}