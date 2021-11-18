using ModuleBased.Example.MainPage;
using ModuleBased.Example.Scenes;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using System.Collections;
using UnityEngine;

namespace ModuleBased.Example.GameFlow
{   
    [Injectable(typeof(GameFlowModule))]
    public class GameFlowModule : MonoBehaviour
    {
        [Inject]
        private IFsm<IGameCore> _fsm;
        [Inject]
        private ISceneModule _sceneMod;
        [Inject]
        private LoadingView _loadingView;


        public void ChangeState<T>() where T : GameFlowStateBase
        {
            var state = Activator.CreateInstance<T>();
            ChangeState(state);
        }

        public void ChangeState(GameFlowStateBase flowState)
        {
            var prevState = _fsm.Current as GameFlowStateBase;
            // enter loading scene
            // the current state will invoke OnExit method
            LoadingState loading = new LoadingState();
            _fsm.SetState(loading);
            if (prevState != null)
            {
                Debug.Log($"Unload {StateName(prevState)}");
                // unload prev state scene
                _sceneMod.UnloadScene(StateName(prevState));
            }
            // wait for next scene loading
            StartCoroutine(LoadNextState(flowState));
        }

        private IEnumerator LoadNextState(GameFlowStateBase flowState)
        {
            var enumerator = flowState.OnEnterLoading(_fsm, _loadingView);
            while (enumerator.MoveNext())
            {
                yield return null;
            }
            // how to wait module initialize?
            _fsm.SetState(flowState);
        }

        private string StateName(GameFlowStateBase state)
        {
            string name = state.Name;
            string stateName = name.Replace("State", "");
            return stateName;
        }

    }

    public class GameFlowStateBase : IFsmState<IGameCore>
    {
        public string Name {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// Execute before enter
        /// </summary>
        public virtual IEnumerator OnEnterLoading(IFsm<IGameCore> fsm, IProgress<float> progress)
        {
            progress.Report(1);
            yield return null;
        }

        public virtual void OnEnter(IFsm<IGameCore> fsm) { }

        public virtual void OnExit(IFsm<IGameCore> fsm) { }

        public virtual void OnUpdate(IFsm<IGameCore> fsm) { }
    }

    public class MainPageState : GameFlowStateBase
    {

        public override IEnumerator OnEnterLoading(IFsm<IGameCore> fsm, IProgress<float> progress)
        {
            float timer = 0;
            float time = 1f;
            while(timer < time)
            {
                timer += Time.deltaTime;
                progress.Report(timer / time);
                yield return null;
            }
        }

        public override void OnEnter(IFsm<IGameCore> fsm)
        {
            // enable needed module
            Debug.Log("Enter main page");
            // how to insert IObservable
            var viewObservable = fsm.Target.Get(typeof(IObservable<MainPageView>)) as IObservable<MainPageView>;
            viewObservable.Subscribe(
                (view) =>
                {
                    Debug.Log($"view is null? {view == null}");
                },
                (ex) =>
                {
                    Debug.LogError(ex);
                },
                () =>
                {
                    Debug.Log("Completed");
                });
        }

        public override void OnExit(IFsm<IGameCore> fms)
        {
            // disable need module
            Debug.Log("Exit main page");
        }
    }

    public class LoadingState : GameFlowStateBase
    {
        public override void OnEnter(IFsm<IGameCore> fsm)
        {
            Debug.Log("enter loading");
            if (!fsm.Target.TryGet(typeof(LoadingView), out object concrete))
                return;
            var view = (LoadingView)concrete;
            view.BeginLoading();
        }

        public override void OnExit(IFsm<IGameCore> fsm)
        {
            Debug.Log("exit loading");
            if (!fsm.Target.TryGet(typeof(LoadingView), out object concrete))
                return;
            var view = (LoadingView)concrete;
            view.EndLoading();
        }
    }

    public class LevelState : GameFlowStateBase
    {
        public override void OnEnter(IFsm<IGameCore> fsm)
        {
            Debug.Log("Enter level");
            var observable = fsm.Target.Get(typeof(IObservable<LevelView>));
            var viewObservable = observable as IObservable<LevelView>;
            viewObservable.Subscribe(
                (view) =>
                {
                    Debug.Log($"view is null? {view == null}");
                },
                (ex) =>
                {
                    Debug.LogError(ex);
                },
                () =>
                {
                    Debug.Log("Completed");
                });
        }

        public override void OnExit(IFsm<IGameCore> fms)
        {
            Debug.Log("Exit level");
        }
    }

}