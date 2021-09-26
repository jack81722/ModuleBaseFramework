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
            _core.Modules.AddModule(typeof(IMyModule), proxy);
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
        

        public void InstantiateCore()
        {
            if (_logger == null)
                _logger = new UniLogger();
            if (_core == null)
                _core = new GameCore(_logger);

        }

        public void StartCore()
        {
            _core.StartCore();
            
        }
    }
}