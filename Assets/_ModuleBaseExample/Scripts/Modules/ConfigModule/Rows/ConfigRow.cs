using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    public class ConfigRow<T>
    {
        public string Key;
        public T Value;
    }

    [Serializable]
    public class ConfigStringRow : ConfigRow<string> { }

    [Serializable]
    public class ConfigFloatRow : ConfigRow<float> { }

    [Serializable]
    public class ConfigIntRow : ConfigRow<int> { }

    [Serializable]
    public class ConfigObjectRow : ConfigRow<UnityEngine.Object> { }

    [Serializable]
    public class ConfigBoolRow : ConfigRow<bool> { }
}
