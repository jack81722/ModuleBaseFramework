using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cheap2.Plugin.Pool
{
    public abstract class UniPool<T> : MonoBehaviour, IPool<T> where T : Component
    {
        public T Prefab;
        public int BatchSpawnSize = 8;
        public abstract int UsableCount { get; }

        public abstract T Pop();

        public abstract void Push(T item);

        public virtual void Spawn(int count)
        {
            for(int i = 0; i < count;i++)
            {
                var item  = Instantiate(Prefab);
                OnSpawn(item);
                Push(item);
            }
        }

        public void Push(object item)
        {
            Push((T)item);
        }

        object IPool.Pop()
        {
            return Pop();
        }

        protected virtual void OnSpawn(T item) { }
    }

    public abstract class UniCollectionPool<T> : UniPool<T> where T : Component
    {
        protected ICollection<T> collection;

        public override int UsableCount => collection.Count;

        public override T Pop()
        {
            if (UsableCount < 1)
                Spawn(BatchSpawnSize);
            var item = collection.Last();
            collection.Remove(item);
            return item;
        }

        public override void Push(T item)
        {
            if (collection.Contains(item))
                return;
            collection.Add(item);
        }
    }
}