using ModuleBased.Example.GameFlow;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ModuleBased.Example.MainPage
{
    public class MainPageView : MonoBehaviour, IDisposable
    {
        [Inject]
        private GameFlowModule _gameFlow;

        private IFsm<MainPageView> _fsm;

        [SerializeField]
        private Button _btnPlay;
        [SerializeField]
        private Button _btnLoad;
        [SerializeField]
        private Button _btnGallery;
        [SerializeField]
        private Button _btnCredit;
        [SerializeField]
        protected WindowView _loadWindow;
        [SerializeField]
        protected WindowView _galleryWindow;
        [SerializeField]
        protected WindowView _creditWindow;

        private void Start()
        {
            _btnPlay.onClick.AddListener(ClickPlay);
            _btnLoad.onClick.AddListener(ClickLoad);
            _btnGallery.onClick.AddListener(ClickGallery);
            _btnCredit.onClick.AddListener(ClickCredit);
            _fsm = new MainPageFsm(this);
            _fsm.SetState<MainPageView, MainPage_IdleState>();
        }


        public void ClickPlay()
        {
            _gameFlow.ChangeState<DialogState>();
        }

        public void ClickLoad()
        {
            _fsm.SetState<MainPageView, MainPage_LoadState>();
        }

        public void ClickGallery()
        {
            _fsm.SetState<MainPageView, MainPage_GalleryState>();
        }

        public void ClickCredit()
        {
            _fsm.SetState<MainPageView, MainPage_CreditState>();
        }

        public void Dispose()
        {
            SceneManager.UnloadSceneAsync("MainPage");
        }

        #region -- Main page fsm --
        protected class MainPageFsm : IFsm<MainPageView>
        {
            public MainPageView Target { get; }

            public IFsmState<MainPageView> Current { get; private set; }

            public MainPageFsm(MainPageView view)
            {
                Target = view;
            }

            public bool IsState<TState>(bool inherit = false) where TState : IFsmState<MainPageView>
            {
                var currentType = Current.GetType();
                return currentType == typeof(TState) || currentType.IsSubclassOf(typeof(TState));
            }

            public void OnError(Exception ex)
            {

            }

            public void SetState(IFsmState<MainPageView> concreteState)
            {
                try
                {
                    Current.OnExit(this);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
                Current = concreteState;
                try
                {
                    Current.OnEnter(this);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }

        protected abstract class MainPageState : IFsmState<MainPageView>
        {
            public virtual void OnEnter(IFsm<MainPageView> fsm) { }

            public virtual void OnExit(IFsm<MainPageView> fms) { }

            public virtual void OnUpdate(IFsm<MainPageView> fsm) { }
        }

        protected class MainPage_IdleState : MainPageState
        {
            public override void OnEnter(IFsm<MainPageView> fsm)
            {
                fsm.Target._galleryWindow.Close();
                fsm.Target._loadWindow.Close();
                fsm.Target._creditWindow.Close();
            }
        }

        protected class MainPage_GalleryState : MainPageState
        {
            public override void OnEnter(IFsm<MainPageView> fsm)
            {
                fsm.Target._loadWindow.Close();
                fsm.Target._creditWindow.Close();
                fsm.Target._galleryWindow.Open().Subscribe(
                    (window) => { },
                    (err) => { },
                    () =>
                    {
                        if (fsm.IsState<MainPage_IdleState>())
                            return;
                        fsm.SetState<MainPageView, MainPage_IdleState>();
                    });
            }
        }

        protected class MainPage_LoadState : MainPageState
        {
            public override void OnEnter(IFsm<MainPageView> fsm)
            {
                fsm.Target._galleryWindow.Close();
                fsm.Target._creditWindow.Close();
                fsm.Target._loadWindow.Open().Subscribe(
                    (window) => { },
                    (err) => { },
                    () =>
                    {
                        if (fsm.IsState<MainPage_IdleState>())
                            return;
                        fsm.SetState<MainPageView, MainPage_IdleState>();
                    });
            }
        }

        protected class MainPage_CreditState : MainPageState
        {
            public override void OnEnter(IFsm<MainPageView> fsm)
            {
                fsm.Target._loadWindow.Close();
                fsm.Target._galleryWindow.Close();
                fsm.Target._creditWindow.Open().Subscribe(
                    (window) => { },
                    (err) => { },
                    () =>
                    {
                        if (fsm.IsState<MainPage_IdleState>())
                            return;
                        fsm.SetState<MainPageView, MainPage_IdleState>();
                    });
            }
        }
        #endregion
    }
}