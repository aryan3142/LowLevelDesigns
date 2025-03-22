namespace LowLevelDesigns.StorageEngine
{
    public class LRUCache
    {
        private readonly int capacity;
        private LinkedList<(string key, string value)> lru = new();
        private Dictionary<string, LinkedListNode<(string key, string value)>> cache = new();
        public LRUCache(int capacity)
        {
            this.capacity = capacity;
        }

        public string Get(string key)
        {
            if (cache.ContainsKey(key))
            {
                string value = cache[key].Value.value;
                lru.Remove(cache[key]);
                lru.AddFirst(cache[key]);
                return value;
            }

            return string.Empty;
        }

        public void AddKey(string key, string value)
        {
            if (cache.ContainsKey(key))
            {
                cache[key].Value =(key,value);
                lru.Remove(cache[key]);
                lru.AddFirst(cache[key]);
            }
            else
            {
                if (lru.Count == capacity)
                {
                    lru.RemoveLast();
                }

                lru.AddFirst((key, value));
            }
        }
    }
}
