using ModuleBased.ForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    [UniDao(typeof(IDialogueDao))]
    [CreateAssetMenu(fileName = "Scenario", menuName = "Dialogue/DialgueScript")]
    public class DialogueScript : ScriptableObject, IDialogueDao {
        [SerializeField]
        private List<BlockInfo> Blocks;

        public BlockInfo GetBlock(string blockName) {
            return Blocks.Find(b => b.BlockName == blockName);
        }

        public bool TryGetBlock(string blockName, out BlockInfo blockInfo) {
            int index = Blocks.FindIndex(b => b.BlockName == blockName);
            if (index < 0)
                blockInfo = new BlockInfo();
            else
                blockInfo = Blocks[index];
            return index >= 0;
        }

        public IEnumerable<BlockInfo> GetBlocks() {
            return Blocks;
        }
    }

    [Serializable]
    public struct BlockInfo {
        public string BlockName;
        public CommandInfo[] CmdInfos;
    }

    [Serializable]
    public struct CommandInfo {
        public string CharacterName;
        public string SayText;
    }

    public struct CommandInfo2 {
        public string CommandName;
        public string[] Args;
    }

    public interface IDialogueDao {
        BlockInfo GetBlock(string blockName);

        bool TryGetBlock(string blockName, out BlockInfo blockInfo);

        IEnumerable<BlockInfo> GetBlocks();
    }
}