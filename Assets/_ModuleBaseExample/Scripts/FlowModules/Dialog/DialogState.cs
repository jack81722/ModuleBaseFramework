using ModuleBased.Example.Drama.Dialog;
using ModuleBased.Example.GameFlow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.Example
{
    public class DialogState : GameFlowStateBase
    {
        List<KeyValuePair<Type, object>> _injectableList;

        public override IEnumerator OnEnterLoading(IFsm<IGameCore> fsm, IProgress<float> progress)
        {
            progress.Report(0.7f);
            _injectableList = new List<KeyValuePair<Type, object>>();
            var asyncOp = SceneManager.LoadSceneAsync("Dialog", LoadSceneMode.Additive);
            while (!asyncOp.isDone)
                yield return null;
            progress.Report(1f);
        }

        public override void OnEnter(IFsm<IGameCore> fsm)
        {
            Debug.Log("Dialog!");
        }

        public override void OnExit(IFsm<IGameCore> fsm)
        {
            foreach (var injectable in _injectableList)
            {
                fsm.Target.Remove(injectable.Key, injectable.Value);
            }
        }
    }
}
