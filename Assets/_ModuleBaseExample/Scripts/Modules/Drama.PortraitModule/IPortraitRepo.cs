using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Portrait
{
    public interface IPortraitRepo 
    {
        Sprite FindByPortraitName(string name);
    }
}
