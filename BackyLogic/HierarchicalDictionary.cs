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
            var path = Path.FromItemPath(itemPath);
            var container = this.GetContainerByPath(path, true);
            container._items.Add(path.ItemKey, item);
        }

        public bool Contains(params TKey[] itemPath)
        {
            var path = Path.FromItemPath(itemPath);
            var container = this.GetContainerByPath(path, false);
            if (container == null)
                return false;
            var ret = container._items.ContainsKey(path.ItemKey);
            return ret;
        }

        public IEnumerable<TValue> GetFirstLevelFiles(params TKey[] containerPath)
        {
            var ret = new List<TValue>();
            var container = this.GetContainerByPath(Path.FromContainerPath(containerPath), false);
            if (container != null)
                ret.AddRange(container._items.Values);
            return ret;
        }

        public IEnumerable<TKey> GetFirstLevelDirectories(params TKey[] containerPath)
        {
            var ret = new List<TKey>();
            var container = this.GetContainerByPath(Path.FromContainerPath(containerPath), false);
            if (container != null)
                ret.AddRange(container._containers.Keys);
            return ret;
        }


        private HierarchicalDictionary<TKey, TValue> GetContainerByPath(Path path, bool createIfMissing)
        {
            var ret = this;
            foreach (TKey key in path.ContainerPath)
            {
                HierarchicalDictionary<TKey, TValue> nextContainer;
                if (ret._containers.TryGetValue(key, out nextContainer))
                {
                    ret = nextContainer;
                }
                else
                {
                    if (!createIfMissing)
                        return null;
                    else {
                        nextContainer = new HierarchicalDictionary<TKey, TValue>();
                        ret._containers.Add(key, nextContainer);
                        ret = nextContainer;
                    }
                }
            }

            return ret;
        }

        public void Remove(TKey[] itemPath)
        {
            var path = Path.FromItemPath(itemPath);
            var container = this.GetContainerByPath(path, false);
            if (container == null)
                throw new ApplicationException("Item not found, cannot be removed");
            container._items.Remove(path.ItemKey);
        }

        public TValue GetFileOrDefault(TKey[] itemPath)
        {
            var path = Path.FromItemPath(itemPath);
            var container = this.GetContainerByPath(path, false);
            if (container == null)
                return default(TValue);

            TValue ret;
            container._items.TryGetValue(path.ItemKey, out ret);
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




        private class Path
        {
            public IEnumerable<TKey> ContainerPath { get; private set; }
            public TKey ItemKey { get; private set; }

            public static Path FromItemPath(params TKey[] itemPath)
            {
                var ret = new Path();
                ret.ContainerPath = itemPath.Take(itemPath.Length - 1);
                ret.ItemKey = itemPath[itemPath.Length - 1];
                return ret;
            }

            public static Path FromContainerPath(params TKey[] containerPath)
            {
                var ret = new Path();
                ret.ContainerPath = containerPath;
                ret.ItemKey = default(TKey);
                return ret;
            }
        }
    }
}
