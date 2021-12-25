using DG.Tweening;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using UnityEngine;

namespace ModuleBased.Example.Drama.Background
{
    [Injectable(typeof(IBackgroundModule))]
    public class BackgroundModule : MonoBehaviour, IBackgroundModule, IEventInject
    {
        #region -- Required --
        [Inject]
        private IBackgroundView _view;

        [Inject]
        private IDramaModule _dramaModule;
        #endregion

        [SerializeField]
        private Sprite _sprite;

        public IDramaAction BackgroundChange(string src, float duration, string ease)
        {
            Ease e = Ease.Linear;
            switch (ease)
            {
                case "linear":
                    e = Ease.Linear;
                    break;
                case "in":
                    e = Ease.InCubic;
                    break;
                case "out":
                    e = Ease.OutCubic;
                    break;
                case "inout":
                    e = Ease.InOutCubic;
                    break;
            }
            var tween = _view.Lerp(_sprite, duration, e);
            return new TweenDramaAction(tween);
        }

        public IDramaAction BackgroundShake(float duration, float strength)
        {
            var tween = _view.Shake(duration, strength);
            return new TweenDramaAction(tween);
        }

        public void OnInject()
        {
            _dramaModule.RegisterAction("BackgroundChange", (args) =>
            {
                string src = args[0];
                float dur = float.Parse(args[1]);
                string ease = args[2];
                return BackgroundChange(src, dur, ease);
            });
        }
    }

    public interface IBackgroundModule
    {
        IDramaAction BackgroundChange(string src, float duration, string ease);

        IDramaAction BackgroundShake(float duration, float strength);
    }
}
