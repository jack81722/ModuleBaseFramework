using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.CommandLine
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ValueAttribute : Attribute
    {
        public object Default { get; }
        public string Name { get; }
        public string HelpText { get; }

        public ValueAttribute(string Name, object Default = null, string HelpText = "")
        {
            this.Name = Name;
            this.Default = Default;
            this.HelpText = HelpText;
        }
    }
}
