#if UNITY_EDITOR
using ModuleBased.Dialogue.FungusPlugin.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ModuleBased.Example {
    [CustomEditor(typeof(CharacterCmd))]
    public class CharacterCmdEditor : GenericCmdEditor<CharacterModule> {
        
    }
}
#endif