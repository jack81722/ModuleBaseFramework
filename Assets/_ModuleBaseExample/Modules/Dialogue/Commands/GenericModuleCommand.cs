using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class GenericModuleCommand : DefaultCommand, IGenericModuleCommand {
        private IGameModule _module;
        private MethodInfo _method;
        private object[] _params;

        public GenericModuleCommand(IGameModule module, MethodInfo method) {
            _module = module;
            _method = method;
        }

        public void SetParameters(object[] parameters) {
            _params = parameters;
        }

        public override IEnumerator Execute() {
            if (typeof(IEnumerator).IsAssignableFrom(_method.ReturnType)) {
                _method.Invoke(_module, _params);
                return null;
            } else {
                return (IEnumerator)_method.Invoke(_module, _params);
            }
        }

        public object Clone() {
            return new GenericModuleCommand(_module, _method);
        }
    }
}