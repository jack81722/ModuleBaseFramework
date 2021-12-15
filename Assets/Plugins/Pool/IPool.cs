using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheap2.Plugin.Pool
{
    public interface IPool
    {
        int UsableCount { get; }

        void Spawn(int count);

        object Pop();

        void Push(object item);
    }

    public interface IPool<T> : IPool
    {
        new T Pop();

        void Push(T item);
    }
}