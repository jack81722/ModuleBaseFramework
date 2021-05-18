using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.DAO {
    public class DefaultGameDaoCollection : IGameDaoCollection {
        private Dictionary<Type, object> _daos = new Dictionary<Type, object>();

        public void AddDao<TItf, TDao>()
            where TItf : class
            where TDao : TItf {
            AddDao(typeof(TItf), typeof(TDao));
        }

        public void AddDao<TItf, TDao>(TDao instance)
            where TItf : class
            where TDao : TItf {
            Type itfType = typeof(TItf);
            AddDao(itfType, instance);
        }

        public void AddDao(Type itfType, Type daoType) {
            if (!itfType.IsAssignableFrom(daoType))
                throw new ArgumentException($"{daoType.Name} must implement {itfType}");
            object inst = Activator.CreateInstance(daoType);
            AddDao(itfType, inst);
        }

        public void AddDao(Type itfType, object daoInst) {
            _daos.Add(itfType, daoInst);
        }

        public TItf GetDao<TItf>() where TItf : class {
            return GetDao(typeof(TItf)) as TItf;
        }

        public object GetDao(Type itfType) {
            if (!itfType.IsInterface)
                throw new ArgumentException("Type must be interface.");
            if (_daos.TryGetValue(itfType, out object inst))
                return inst;
            else
                return null;
        }

        public IEnumerable GetAllDaos() {
            return _daos.Values;
        }
    }
}