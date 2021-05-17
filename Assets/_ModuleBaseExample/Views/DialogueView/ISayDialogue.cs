using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.DialogueViews {
    public delegate void EndSayCallback();

    public interface ISayWindow {

        string CharacterName { get; set; }
        string SayText { get; set; }

        void BeginSay(string sayText, EndSayCallback endSay = null);

        void BeginSay(string charName, string sayText, EndSayCallback endSay = null);

        /// <summary>
        /// Show all text immediately
        /// </summary>
        void ShowAllTextImmediatly();
    }
}