using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Dialogue {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ModuleCmdAttribute : Attribute {
        public string CmdName { get; }

        public ModuleCmdAttribute(string name = "") {
            CmdName = name;
        }
    }
}