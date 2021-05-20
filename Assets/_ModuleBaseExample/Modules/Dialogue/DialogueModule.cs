using ModuleBased.Dialogue;
using ModuleBased.ForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    /// <summary>
    /// Dialogue Module is a view module
    /// </summary>
    [ModuleItf(typeof(IDialogueModule))]
    public class DialogueModule : MonoBehaviour, IGameModule, IDialogueModule {
        private Dictionary<string, IDialogueBlock> _blockCache;
        private Coroutine _currentBlockCoroutine;
        public IGameModuleCollection Modules { get; set; }

        [RequireDao]
        private IDialogueDao _dialogueDao;

        #region -- IGameModule --
        public void OnModuleInitialize() {
            _blockCache = new Dictionary<string, IDialogueBlock>();
        }

        public void OnModuleStart() {
            InitializeModuleCmds();
            Test();
        }
        #endregion

        #region -- Unity APIs --
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space))
                StartBlock("FirstMeet");
        }
        #endregion

        private void Test() {
            var block = CreateBlock("test");
            block.AddCommand(new SayCommand("Coinmouse", "Hello XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"));
            var parallel = new ParallelDialogueBlock();
            parallel.AddCommand(CreateModuleCommand("SetWeather", EWeatherState.Rain));
            parallel.AddCommand(new SayCommand("Coinmouse", "............................................."));
            block.AddCommand(parallel);
            //block.AddCommand(CreateModuleCommand("SetRain"));
            block.AddCommand(new WaitSecondCommand() { WaitSecond = 2 });
            block.AddCommand(new SayCommand("Coinmouse", "OMG"));
            StartBlock(block);
        }

        public IDialogueBlock CreateBlock(string blockName) {
            if (_dialogueDao.TryGetBlock(blockName, out BlockInfo blockInfo)) {
                return CreateBlock(blockInfo);
            }
            return new DialogueBlock();
        }

        public IDialogueBlock CreateBlock(BlockInfo blockInfo) {

            DialogueBlock block = new DialogueBlock();
            string name, sayText;
            foreach (var dialog in blockInfo.CmdInfos) {
                name = dialog.CharacterName;
                sayText = dialog.SayText;
                SayCommand command = new SayCommand(name, sayText);
                block.AddCommand(command);
            }
            return block;
        }

        public IDialogueCommand CreateModuleCommand(string cmdName, params object[] parameters) {
            _modCmds.TryGetValue(cmdName, out IGenericModuleCommand cmd);
            IGenericModuleCommand clonedCmd = (IGenericModuleCommand)cmd.Clone();
            clonedCmd.SetParameters(parameters);
            return clonedCmd;
        }

        public void StartBlock(string blockName) {
            if (_currentBlockCoroutine != null)
                StopCoroutine(_currentBlockCoroutine);
            if (!_blockCache.TryGetValue(blockName, out IDialogueBlock block)) {
                block = CreateBlock(blockName);
                if (block != null)
                    _blockCache.Add(blockName, block);
            }
            _currentBlockCoroutine = StartCoroutine(block.StartExecution());
        }

        public void StartBlock(IDialogueBlock block) {
            if (_currentBlockCoroutine != null)
                StopCoroutine(_currentBlockCoroutine);
            _currentBlockCoroutine = StartCoroutine(block.StartExecution());
        }

        #region -- Module command methods --
        private Dictionary<string, IGenericModuleCommand> _modCmds;

        private void InitializeModuleCmds() {
            _modCmds = new Dictionary<string, IGenericModuleCommand>();
            Type itfType;
            foreach (var pair in Modules) {
                itfType = pair.Key;
                foreach (var method in itfType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                    if (method.IsDefined(typeof(ModuleCmdAttribute), true)) {
                        var attr = method.GetCustomAttribute<ModuleCmdAttribute>();
                        GenericModuleCommand cmd = new GenericModuleCommand(pair.Value, method);
                        string cmdName = string.IsNullOrEmpty(attr.CmdName) ? method.Name : attr.CmdName;
                        _modCmds.Add(cmdName, cmd);
                    }
                }
            }
        }
        #endregion
    }

  
}