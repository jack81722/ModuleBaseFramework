using Cheap2.Plugin.Pool;
using DG.Tweening;
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
        private IDramaModule _dramaModule;
        #endregion

        private Dictionary<string, PortraitAgent> _portraits;

        private void Awake()
        {
            _portraits = new Dictionary<string, PortraitAgent>();
        }


        public PortraitAgent FindPortrait(string id)
        {
            if (!_portraits.TryGetValue(id, out PortraitAgent agent))
            {
                return null;
            }
            return agent;
        }

        #region -- Portrait actions --
        public IDramaAction PortraitNew(string id, string src, float alpha, EPortraitLayout layout)
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
            agent.Alpha = alpha;
            _view.PutPortait(agent, layout);
            agent.Display();
            return new EmptyDramAction();
        }

        public IDramaAction PortraitRemove(string id)
        {
            if (_portraits.TryGetValue(id, out PortraitAgent agent))
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

        public IDramaAction PortraitShake(string id, float duration, float strength)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            return new TweenDramaAction(portrait.DoShake(duration, strength));
        }

        public IDramaAction PortraitAlphaLerp(string id, float from, float to, float duration, Ease ease)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            portrait.Alpha = from;
            return new TweenDramaAction(portrait.DoFade(to, duration).SetEase(ease));
        }

        public IDramaAction PortraitChange(string id, string src)
        {
            var portrait = FindPortrait(id);
            if (portrait == null)
                return null;
            var sprite = _repo.FindByPortraitName(src);
            portrait.SetSprite(sprite);
            return new EmptyDramAction();
        }
        #endregion

        private EPortraitLayout LayoutFromString(string str)
        {
            EPortraitLayout layout;
            switch (str.ToLower())
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
            return layout;
        }

        public void OnInject()
        {
            _dramaModule.RegisterAction(nameof(PortraitNew), (args) =>
            {
                string id = args[0];
                string src = args[1];
                float alpha = float.Parse(args[2]);
                EPortraitLayout layout = LayoutFromString(args[3]);
                return PortraitNew(id, src, alpha, layout);
            });
            _dramaModule.RegisterAction(nameof(PortraitShake), (args) =>
            {
                string id = args[0];
                float duration = float.Parse(args[1]);
                float strength = float.Parse(args[2]);
                return PortraitShake(id, duration, strength);
            });
            _dramaModule.RegisterAction(nameof(PortraitRemove), (args) =>
            {
                string id = args[0];
                return PortraitRemove(id);
            });
            _dramaModule.RegisterAction(nameof(PortraitAlphaLerp), (args) =>
            {
                string id = args[0];
                float from = float.Parse(args[1]);
                float to = float.Parse(args[2]);
                float duration = float.Parse(args[3]);
                Ease ease = args[4].EaseFromString();
                return PortraitAlphaLerp(id, from, to, duration, ease);
            });
            _dramaModule.RegisterAction(nameof(PortraitChange), (args) =>
            {
                string id = args[0];
                string src = args[1];
                return PortraitChange(id, src);
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
        IDramaAction PortraitNew(string id, string src, float alpha, EPortraitLayout layout);

        IDramaAction PortraitShake(string id, float duration, float strength);
    }
}
