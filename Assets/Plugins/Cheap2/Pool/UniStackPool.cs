using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheap2.Plugin.Pool
{
    public class UniStackPool<T> : UniPool<T> where T : Component
    {
        protected Stack<T> stack;

        public override int UsableCount => stack.Count;

        public override T Pop()
        {
            if (UsableCount < 1)
                Spawn(BatchSpawnSize);
            var item = stack.Pop();
            OnPop(item);
            return item;
        }

        public override void Push(T item)
        {
            if (stack.Contains(item))
                return;
            stack.Push(item);
            OnPush(item);
        }

        private void Awake()
        {
            stack = new Stack<T>();
        }

        protected virtual void OnPop(T item) { }

        protected virtual void OnPush(T item) { }

    }
}