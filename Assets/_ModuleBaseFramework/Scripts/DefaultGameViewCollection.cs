using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased {
    public class DefaultGameViewCollection : IGameViewCollection {
        private ILogger _logger;
        private IGameModuleCollection _modules;

        private Dictionary<Type, IGameView> _views;

        public DefaultGameViewCollection(ILogger logger, IGameModuleCollection modules) {
            _logger = logger;
            _modules = modules;
            _views = new Dictionary<Type, IGameView>();
        }

        public void AddView(IGameView view) {
            Type type = view.GetType();
            if (!_views.ContainsKey(type))
                _views.Add(type, view);
            else
                _logger.LogError($"Duplicate game view.");
        }

        public void InitializeViews() {
            foreach (var view in _views.Values) {
                view.Logger = _logger;
                view.Modules = _modules;
                AssignRequiredModulesToView(view);
            }
        }

        private void AssignRequiredModulesToView(IGameView view) {
            Type viewType = view.GetType();
            MemberInfo[] members = viewType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members) {
                if (member.IsDefined(typeof(RequireModuleAttribute))) {
                    if (member.MemberType == MemberTypes.Field) {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(view, _modules.GetModule(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(view, _modules.GetModule(reqType));
                    }
                }

            }
        }
    }
}