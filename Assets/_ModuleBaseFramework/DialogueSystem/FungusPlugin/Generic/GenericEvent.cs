using Fungus;
using ModuleBased.ForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EventHandler = Fungus.EventHandler;

namespace ModuleBased.Dialogue.FungusPlugin {
    public abstract class GenericEvent<TItf> : EventHandler where TItf : class {
        private static readonly Type CmdAttr = typeof(ModuleEventAttribute);

        private static TItf module;
        private static IDictionary<string, EventInfo> events;

        [SerializeField]
        private string EventName;
        private Delegate _handler;

        private void Start() {
            if (module == null)
                module = UniGameCore.Singleton.GetModule<TItf>();
            events = ModuleEventCache<TItf>.GetEvents();

            // register event
            if (events.TryGetValue(EventName, out EventInfo e)) {
                _handler = Delegate.CreateDelegate(e.EventHandlerType, this, "Listen_OnEvent");
                e.AddEventHandler(module, _handler);
            } else {
                Debug.LogError($"Event ({EventName}) not found");
            }
        }

        private void Listen_OnEvent() {
            ExecuteBlock();
        }

        private void OnDisable() {
            foreach (var e in events.Values) {
                e.RemoveEventHandler(module, _handler);
            }
        }

        public static void InitializeModuleEvents(ref Dictionary<string, EventInfo> events) {
            events = new Dictionary<string, EventInfo>();
            Type modType = typeof(TItf);
            EventInfo[] methodInfos = modType.GetEvents(BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in methodInfos) {
                if (info.IsDefined(CmdAttr)) {
                    ModuleEventAttribute attr = info.GetCustomAttribute(CmdAttr, true) as ModuleEventAttribute;
                    string cmdName = string.IsNullOrEmpty(attr.EventName) ? info.Name : attr.EventName;
                    if (!events.ContainsKey(cmdName))
                        events.Add(cmdName, info);
                    else
                        Debug.LogError("Duplicate command name.");
                }
            }
        }
    }

    public class ModuleEventCache<TItf> where TItf : class {
        private static Dictionary<string, EventInfo> _events;

        public static IDictionary<string, EventInfo> GetEvents() {
            if (_events == null) {
                InitializeModuleEvents(ref _events);
            }
            return _events;
        }

        public static void InitializeModuleEvents(ref Dictionary<string, EventInfo> events) {
            Type CmdAttr = typeof(ModuleEventAttribute);
            if (events == null)
                events = new Dictionary<string, EventInfo>();
            else
                events.Clear();
            Type modType = typeof(TItf);
            EventInfo[] methodInfos = modType.GetEvents(BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in methodInfos) {
                if (info.IsDefined(CmdAttr)) {
                    ModuleEventAttribute attr = info.GetCustomAttribute(CmdAttr, true) as ModuleEventAttribute;
                    string eventName = string.IsNullOrEmpty(attr.EventName) ? info.Name : attr.EventName;
                    if (!events.ContainsKey(eventName))
                        events.Add(eventName, info);
                    else
                        Debug.LogError("Duplicate command name.");
                }
            }
        }
    }
}