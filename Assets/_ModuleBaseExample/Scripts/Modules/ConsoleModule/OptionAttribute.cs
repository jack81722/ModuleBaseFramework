using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.CommandLine
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OptionAttribute : Attribute
    {
        public string ShortName { get; }
        public string FullName { get; }
        public bool IsRequired { get; }   
        public object Default { get; }
        public string HelpText { get; }

        public OptionAttribute(string ShortName, string FullName, object Default = null, bool IsRequired = false, string HelpText = "")
        {
            this.ShortName = ShortName;
            this.FullName = FullName;
            this.IsRequired = IsRequired;
            this.HelpText = HelpText;
            this.Default = Default;
        }
    }
}
