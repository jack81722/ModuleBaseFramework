using Cheap2.Plugin.Pool;
using ModuleBased.Example.Drama.Dialog;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Portrait
{
    [Injectable(typeof(IPortraitModule))]
    public class PortraitModule : MonoBehaviour, IPortraitModule, IEventInject
    {
        #region -- Required --
        [Inject]
        private IPool<PortraitAgent> _pool;
        [Inject]
        private PortraitView _view;
        [Inject]
        private IPortraitRepo _repo;

        [Inject]
        private IDialogModule _dialogModule;
        #endregion

        private Dictionary<string, PortraitAgent> _portraits;

        private void Awake()
        {
            _portraits = new Dictionary<string, PortraitAgent>();
        }

        private void Start()
        {
            NewPortrait("0", "misaki", EPortraitLayout.Middle);
        }

        public PortraitAgent FindPortrait(string id)
        {
            if (!_portraits.TryGetValue(id, out PortraitAgent agent))
            {
                return null;
            }
            return agent;
        }


        public IDramaAction NewPortrait(string id, string src, EPortraitLayout layout)
        {
            Sprite sprite;
            PortraitAgent agent;
            if (!_portraits.TryGetValue(id, out agent))
            {   
                agent = _pool.Pop();
                _portraits.Add(id, agent);
            }
            sprite = _repo.FindByPortraitName(src);
            agent.SetSprite(sprite);
            _view.PutPortait(agent, layout);
            agent.Display();
            return new EmptyDramAction();
        }

        public IDramaAction RemovePortrait(string id)
        {
            if(_portraits.TryGetValue(id, out PortraitAgent agent))
            {
                agent.Dispose();
            }
            _portraits.Remove(id);
            return new EmptyDramAction();
        }

        public IDramaAction MovePortrait(string id, EPortraitLayout layout)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            _view.PutPortait(portrait, layout);
            return new EmptyDramAction();
        }

        public IDramaAction ShakePortrait(string id, float duration, float strength)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            return new TweenDramaAction(portrait.DoShake(duration, strength));
        }

        public IDramaAction FadeInPortrait(string id, float duration)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            return new TweenDramaAction(portrait.DoFade(1, duration));
        }

        public IDramaAction FadeOutPortrait(string id, float duration)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            return new TweenDramaAction(portrait.DoFade(0, duration));
        }

        public void OnInject()
        {
            _dialogModule.RegisterAction("NewPortrait", (args) =>
            {
                string id = args[0];
                string src = args[1];
                EPortraitLayout layout = EPortraitLayout.Left;
                switch (args[2].ToLower())
                {
                    case "r":
                        layout = EPortraitLayout.Right;
                        break;
                    case "rm":
                        layout = EPortraitLayout.MiddleRight;
                        break;
                    case "m":
                        layout = EPortraitLayout.Middle;
                        break;
                    case "lm":
                        layout = EPortraitLayout.MiddleLeft;
                        break;
                    case "l":
                        layout = EPortraitLayout.Left;
                        break;
                    default:
                        layout = EPortraitLayout.Left;
                        break;
                }
                return NewPortrait(id, src, layout);
            });
            _dialogModule.RegisterAction("ShakePortrait", (args) =>
            {
                string id = args[0];
                float duration = float.Parse(args[1]);
                float strength = float.Parse(args[2]);
                return ShakePortrait(id, duration, strength);
            });
            _dialogModule.RegisterAction("FadeInPortrait", (args) =>
            {
                string id = args[0];
                float duration = float.Parse(args[1]);
                return FadeInPortrait(id, duration);
            });
            _dialogModule.RegisterAction("FadeOutPortrait", (args) =>
            {
                string id = args[0];
                float duration = float.Parse(args[1]);
                return FadeOutPortrait(id, duration);
            });
            _dialogModule.RegisterAction("RemovePortrait", (args) =>
            {
                string id = args[0];
                return RemovePortrait(id);
            });
        }
    }

    

    public enum EPortraitLayout
    {
        Right,
        MiddleRight,
        Middle,
        MiddleLeft,
        Left
    }

    public interface IPortraitModule
    {
        IDramaAction NewPortrait(string id, string src, EPortraitLayout layout);

        IDramaAction ShakePortrait(string id, float duration, float strength);
    }
}
