using Fungus;
using ModuleBased.ForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.FungusPlugin {
    public class GenericCmd<TItf> : Command where TItf : class {
        private static readonly Type CmdAttr = typeof(ModuleCmdAttribute);

        private static TItf module;
        private static IDictionary<string, MethodInfo> methods;

        [SerializeField]
        private string CmdName;

        private IEnumerator _enumerator;

        private void Start() {
            if (module == null) {
                module = UniGameCore.Singleton.GetModule<TItf>();
            }
            methods = ModuleCmdCache<TItf>.GetMethods();
        }

        private void Update() {
            if(_enumerator != null) {
                if (!_enumerator.MoveNext()) {
                    Continue();
                    _enumerator = null;
                }   
            }
        }

        public override void Execute() {
            // check if cmd name is null or empty
            if (string.IsNullOrEmpty(CmdName)) {
                Debug.LogWarning("Command name is null, skip the command.");
            } else {
                // check if cmd name is in dictionary
                if (methods.TryGetValue(CmdName, out MethodInfo method)) {
                    try {
                        if(method.ReturnType == typeof(void))
                            method.Invoke(module, new object[0]);
                        else if(typeof(IEnumerator).IsAssignableFrom(method.ReturnType)) {
                            _enumerator = method.Invoke(module, new object[0]) as IEnumerator;
                        }
                    }
                    catch (Exception e) {
                        Debug.LogError(e);
                    }
                } else {
                    Debug.LogWarning("Command not found, skip the command.");
                }
            }
            if(_enumerator == null)
                Continue();
        }

        #region -- Editor cache --
        public override string CommandName() {
            return GetType().Name + "." + CmdName;
        }
        #endregion
    }

    /// <summary>
    /// Static type cache of module
    /// </summary>
    public class ModuleCmdCache<TItf> where TItf : class {
        private static Dictionary<string, MethodInfo> _methods;

        public static IDictionary<string, MethodInfo> GetMethods() {
            if (_methods == null) {
                InitializeModuleCmds(ref _methods);
            }
            return _methods;
        }

        public static void InitializeModuleCmds(ref Dictionary<string, MethodInfo> methods) {
            Type CmdAttr = typeof(ModuleCmdAttribute);
            if (methods == null)
                methods = new Dictionary<string, MethodInfo>();
            else
                methods.Clear();
            Type modType = typeof(TItf);
            MethodInfo[] methodInfos = modType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in methodInfos) {
                if (info.IsDefined(CmdAttr)) {
                    ModuleCmdAttribute attr = info.GetCustomAttribute(CmdAttr, true) as ModuleCmdAttribute;
                    string cmdName = string.IsNullOrEmpty(attr.CmdName) ? info.Name : attr.CmdName;
                    if (!methods.ContainsKey(cmdName))
                        methods.Add(cmdName, info);
                    else
                        Debug.LogError("Duplicate command name.");
                }
            }
        }
    }
}