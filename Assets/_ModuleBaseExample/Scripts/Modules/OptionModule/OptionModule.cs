using ModuleBased.ForUnity;
using ModuleBased.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.Example
{
    [Injectable(typeof(OptionModule))]
    public class OptionModule : UniGameModule, IEventInject
    {
        public void OnInject()
        {
            var asyncOp = SceneManager.LoadSceneAsync("Option", LoadSceneMode.Additive);
        }

    }
}