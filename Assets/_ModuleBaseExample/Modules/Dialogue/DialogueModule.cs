using ModuleBased.Dialogue;
using ModuleBased.ForUnity;
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

        public void OnModuleInitialize() {
            _blockCache = new Dictionary<string, IDialogueBlock>();
        }

        public void OnModuleStart() {
            //foreach(var mod in Modules) {
                // get all method with module command attribute
                // create and cache cmd
            //}
            StartBlock("FirstMeet");
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space))
                StartBlock("FirstMeet");
        }

        public IDialogueBlock CreateBlock(string blockName) {
            if (_dialogueDao.TryGetBlock(blockName, out BlockInfo blockInfo)) {
                return CreateBlock(blockInfo);
            }
            return null;
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

        #region -- Module command methods --
        private Dictionary<string, IDialogueCommand> _modCmds;

        
        #endregion
    }

    public class GenericModuleCommand : DefaultCommand {
        private IGameModule _module;
        private MethodInfo _method;

        public GenericModuleCommand(IGameModule module, MethodInfo method) {
            _module = module;
            _method = method;
        }

        public override IEnumerator Execute() {
            if (typeof(IEnumerator).IsAssignableFrom(_method.ReturnType)) {
                _method.Invoke(_module, new object[0]);
                return null;
            } else {
                return (IEnumerator)_method.Invoke(_module, new object[0]);
            }
        }
    }
}