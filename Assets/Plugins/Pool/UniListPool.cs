using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cheap2.Plugin.Pool
{
    public class UniListPool<T> : UniCollectionPool<T> where T : Component
    {
        private void Awake()
        {
            collection = new List<T>();
        }

        public IEnumerable<T> Pop(int count)
        {
            if (UsableCount < count)
            {
                Spawn(BatchSpawnSize + count);
            }
            var list = (List<T>)collection;
            var items = list.GetRange(list.Count - count, count);
            list.RemoveRange(list.Count - count, count);
            return items;
        }

        public void Push(IEnumerable<T> items)
        {
            collection = new List<T>(collection.Union(items));
            return;
        }
    }
}