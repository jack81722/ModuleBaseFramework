using System;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModuleBased.ForUnity;

namespace ModuleBased.Example.Drama.Dialog
{
    [Injectable(typeof(ChatBoxPool))]
    [InjectableFactory(typeof(ChatBox), EContractScope.Transient)]
    public class ChatBoxPool : MonoBehaviour, IFactory
    {
        [SerializeField]
        private ChatBox _prefab;
        private Stack<ChatBox> _pool = new Stack<ChatBox>();

        private void Start()
        {
            Spawn(8);
        }

        public object Create(object args)
        {
            if (_pool.Count < 0)
            {
                Spawn(8);
            }
            return _pool.Pop();
        }

        private void Spawn(int count)
        {
            if (count < 1)
                throw new ArgumentException("count must be greater than zero.");
            for (int i = 0; i < count; i++)
            {
                var instance = Instantiate(_prefab, transform);
                instance.Hide();
                _pool.Push(instance);
            }
        }

        public ChatBox Pop()
        {
            return _pool.Pop();
        }

        public void Push(ChatBox item)
        {
            if (_pool.Contains(item))
                return;
            _pool.Push(item);
        }
    }
}
