using System;

namespace ModuleBased.Dialogue {
    [AttributeUsage(AttributeTargets.Event)]
    public class ModuleEventAttribute : Attribute {
        public string EventName { get; }

        public ModuleEventAttribute(string name = "") {
            EventName = name;
        }
    }
}