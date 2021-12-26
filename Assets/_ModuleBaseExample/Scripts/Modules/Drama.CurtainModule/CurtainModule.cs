using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Curtain
{
    [Injectable(typeof(ICurtainModule))]
    public class CurtainModule : MonoBehaviour, ICurtainModule, IEventInject
    {
        #region -- Required --
        [Inject]
        private IDramaModule _drama;
        [Inject]
        private CurtainView _view;
        #endregion


        public IDramaAction CurtainAlphaLerp(float from, float to, float duration)
        {
            _view.Alpha = from;
            return new TweenDramaAction(_view.DoFade(to, duration));
        }

        public IDramaAction CurtainRadialLerp(float from, float to, float duration)
        {
            return new TweenDramaAction(_view.DoRadialLerp(from, to, duration));
        }

        public IDramaAction CurtainColor(string pantone)
        {
            if (!ColorUtility.TryParseHtmlString(pantone, out Color color))
                return null;
            _view.Color = color;
            return new EmptyDramAction();
        }

        public void OnInject()
        {
            _drama.RegisterAction(nameof(CurtainAlphaLerp), (args) =>
            {
                float from = float.Parse(args[0]);
                float to = float.Parse(args[1]);
                float duration = float.Parse(args[2]);
                return CurtainAlphaLerp(from, to, duration);
            });
            _drama.RegisterAction(nameof(CurtainColor), (args) =>
            {
                string pantone = args[0];
                return CurtainColor(pantone);
            });
            _drama.RegisterAction(nameof(CurtainRadialLerp), (args) =>
            {
                float from = float.Parse(args[0]);
                float to = float.Parse(args[1]);
                float duration = float.Parse(args[2]);
                return CurtainRadialLerp(from, to, duration);
            });
        }
    }

    public interface ICurtainModule
    {

    }
}
