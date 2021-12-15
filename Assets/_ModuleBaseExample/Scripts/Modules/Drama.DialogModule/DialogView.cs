using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Dialog
{
    [Injectable(typeof(DialogView))]
    public class DialogView : MonoBehaviour
    {
        [Inject]
        private IDialogModule _dialogMod;


        public void ClickPlay()
        {
            _dialogMod.Load("EroChapter_Prolog.csv");
            _dialogMod.Play();
        }
    }
}
