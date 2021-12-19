using System;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModuleBased.ForUnity;
using Cheap2.Plugin.Pool;

namespace ModuleBased.Example.Drama.Dialog
{
    [Injectable(typeof(IPool<ChatBox>))]
    [InjectableFactory(typeof(ChatBox), EContractScope.Transient)]
    public class ChatBoxPool : UniStackPool<ChatBox>, IFactory
    {
        [Inject]
        private IGameCore _core;

        public object Create(object args)
        {
            return Pop();
        }

        private void Start()
        {
            Spawn(8);
        }

        protected override void OnSpawn(ChatBox item)
        {
            _core.Inject(item);
            item.transform.parent = transform;
            item.Hide();
            item.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            item.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        }
    }
}
