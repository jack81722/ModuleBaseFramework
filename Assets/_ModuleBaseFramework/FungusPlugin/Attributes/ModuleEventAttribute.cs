using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.FungusPlugin {
    [AttributeUsage(AttributeTargets.Event)]
    public class ModuleEventAttribute : Attribute {
        public string EventName { get; }

        public ModuleEventAttribute(string name = "") {
            EventName = name;
        }
    }
}