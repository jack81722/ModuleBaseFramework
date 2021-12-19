using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Portrait
{
    [Injectable(typeof(IPortraitRepo))]
    public class PortraitRepo : MonoBehaviour, IPortraitRepo
    {
        [SerializeField]
        private Sprite sprite;

        public Sprite FindByPortraitName(string name)
        {
            return sprite;
        }
    }
}
