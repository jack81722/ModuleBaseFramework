using DG.Tweening;
using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example.Drama.Background
{
    [Injectable(typeof(IBackgroundView))]
    public class BackgroundView : MonoBehaviour, IBackgroundView
    {
        [SerializeField]
        private Image _primaryImg;
        [SerializeField]
        private Image _secondaryImg;
        private Sprite _lerpingSprite;

        public Tween Shake(float duration, float strength)
        {
            var tween = transform
                .DOShakePosition(duration, strength)
                .SetAutoKill(false);
            return tween;
        }

        public Tween Lerp(Sprite sprite, float duration, Ease ease)
        {
            _lerpingSprite = sprite;
            _secondaryImg.enabled = true;
            var color = _secondaryImg.color;
            color.a = 0;
            _secondaryImg.color = color;
            _secondaryImg.overrideSprite = _secondaryImg.sprite = sprite;
            var tween = _secondaryImg
                .DOFade(1, duration)
                .SetEase(ease)
                .SetAutoKill(false);
            tween.onComplete += onLerpEnd;
            return tween;
        }

        private void onLerpEnd()
        {
            _primaryImg.overrideSprite = _primaryImg.sprite = _lerpingSprite;
            var color = _secondaryImg.color;
            color.a = 1;
            _secondaryImg.color = color;
            _secondaryImg.enabled = false;
        }
    }

    public interface IBackgroundView
    {
        Tween Shake(float duration, float strength);

        Tween Lerp(Sprite sprite, float duration, Ease ease);
    }
}
