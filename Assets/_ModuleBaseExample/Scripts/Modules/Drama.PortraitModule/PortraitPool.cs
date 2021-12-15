using Cheap2.Plugin.Pool;
using ModuleBased.Example.Drama.Portrait;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Portrait
{
    [Injectable(typeof(IPool<PortraitAgent>))]
    [InjectableFactory(typeof(PortraitAgent), EContractScope.Transient)]
    public class PortraitPool : UniStackPool<PortraitAgent>, IFactory
    {
        private void Start()
        {
            Spawn(BatchSpawnSize);
        }

        protected override void OnSpawn(PortraitAgent item)
        {
            item.pool = this;
        }

        protected override void OnPush(PortraitAgent item)
        {
            item.transform.parent = transform;
            item.Hide();
            item.Clear();
        }

        public object Create(object args)
        {
            return Pop();
        }
    }
}
