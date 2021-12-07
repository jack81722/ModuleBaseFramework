using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class VerbAttribute : Attribute
    {
        public string Name { get; }

        public VerbAttribute(string Name)
        {
            this.Name = Name;
        }
    }
}
