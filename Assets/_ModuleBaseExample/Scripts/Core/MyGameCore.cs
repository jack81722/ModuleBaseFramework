using System;
using System.Collections;
using System.Collections.Generic;
using ModuleBased.DAO;
using ModuleBased.ForUnity;
using UnityEngine;

namespace ModuleBased.Example
{
    public class MyGameCore : MonoBehaviour, IGameCore
    {
        private static MyGameCore _singleton;
        public static MyGameCore Singleton {
            get
            {
                if (_singleton == null)
                {
                    _singleton = FindObjectOfType<MyGameCore>();

                }
                return _singleton;
            }
        }

        private void Awake()
        {
            InstantiateCore();
            var mod = GetComponent<MyModule>();
            var proxy = new MyModuleProxy(mod);
            AddModule<IMyModule>(proxy);
        }

        private void Start()
        {
            StartCore();
        }


        private IGameCore _core;
        private ILogger _logger;

        public IGameModuleCollection Modules => _core.Modules;

        public IGameViewCollection Views => _core.Views;

        public IGameDaoCollection Daos => _core.Daos;

        public void AddModule(Type itfType, Type modType)
        {
            _core.AddModule(itfType, modType);
        }

        public void AddModule(Type itfType, IGameModule mod)
        {
            _core.AddModule(itfType, mod);
        }

        public void AddModule<TItf, TMod>()
            where TItf : class
            where TMod : IGameModule, TItf
        {
            _core.AddModule<TItf, TMod>();
        }

        public void AddModule<TItf>(IGameModule mod) where TItf : class
        {
            _core.AddModule<TItf>(mod);
        }

        public void InstantiateCore()
        {
            if (_logger == null)
                _logger = new UniLogger();
            if (_core == null)
                _core = new GameCore(_logger, false);

        }

        public void StartCore()
        {
            _core.StartCore();
            
        }
    }
}