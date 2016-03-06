using System;
using System.Collections.Generic;
using System.Linq;


namespace BackyLogic
{
    public class HierarchicalDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
        private Dictionary<TKey, HierarchicalDictionary<TKey, TValue>> _containers = new Dictionary<TKey, HierarchicalDictionary<TKey, TValue>>();


        public void Add(TValue item, params TKey[] itemPath)
        {
            var container = this.GetContainerByPath(itemPath, true);
            container._items.Add(itemPath.Last(), item);
        }

        public bool Contains(params TKey[] itemPath)
        {
            var directory = this.GetContainerByPath(itemPath, false);
            if (directory == null)
                return false;
            var ret = directory._items.ContainsKey(itemPath.Last());
            return ret;
        }

        public IEnumerable<TValue> GetFirstLevelFiles(params TKey[] containerPath)
        {
            var ret = new List<TValue>();
            //if (keyPath.Last() != "") keyPath = keyPath.Concat(new[] { "" }).ToArray();
            var directory = this.GetContainerByPath(containerPath, false);
            if (directory != null)
                ret.AddRange(directory._items.Values);
            return ret;
        }

        public IEnumerable<TKey> GetFirstLevelDirectories(params TKey[] containerPath)
        {
            var ret = new List<TKey>();
            //if (keyPath.Last() != "") keyPath = keyPath.Concat(new[] { "" }).ToArray();
            var directory = this.GetContainerByPath(containerPath, false);
            if (directory != null)
                ret.AddRange(directory._containers.Keys);
            return ret;
        }


        private HierarchicalDictionary<TKey, TValue> GetContainerByPath(TKey[] containerPath, bool createIfMissing)
        {
            var ret = this;
            for (int i = 0; i < containerPath.Length - 1; i++)
            {
                var key = containerPath[i];
                HierarchicalDictionary<TKey, TValue> nextDirectory;
                if (!ret._containers.TryGetValue(key, out nextDirectory))
                {
                    if (!createIfMissing)
                        return null;
                    else {
                        nextDirectory = new HierarchicalDictionary<TKey, TValue>();
                        ret._containers.Add(key, nextDirectory);
                    }
                }
                ret = nextDirectory;
            }

            return ret;
        }

        public void Remove(TKey[] itemPath)
        {
            var directory = this.GetContainerByPath(itemPath, false);
            if (directory == null)
                throw new ApplicationException("Item not found, cannot be removed");
            directory._items.Remove(itemPath.Last());
        }

        public TValue GetFileOrNull(TKey[] itemPath)
        {
            var directory = this.GetContainerByPath(itemPath, false);
            if (directory == null)
                return default(TValue);

            TValue ret;
            directory._items.TryGetValue(itemPath.Last(), out ret);
            return ret;
        }

        public IEnumerable<TValue> GetAllItems()
        {
            var ret = new List<TValue>();
            var q = new Queue<HierarchicalDictionary<TKey, TValue>>();
            q.Enqueue(this);
            while (q.Count > 0)
            {
                var curentItem = q.Dequeue();
                ret.AddRange(curentItem._items.Values);
                foreach (var innerItem in curentItem._containers.Values)
                    q.Enqueue(innerItem);
            }
            return ret;
        }
    }
}
