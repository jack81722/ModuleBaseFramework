#if UNITY_EDITOR
using ModuleBased.Dialogue.FungusPlugin.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ModuleBased.Example {
    [CustomEditor(typeof(WeatherCmd))]
    public class WeatherCmdEditor : GenericCmdEditor<WeatherModule> { }

    
}
#endif