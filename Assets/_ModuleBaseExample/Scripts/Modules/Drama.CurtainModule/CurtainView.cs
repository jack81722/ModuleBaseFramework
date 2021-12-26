using DG.Tweening;
using ModuleBased.Example.Drama;
using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example.Drama.Curtain
{
    [Injectable(typeof(CurtainView))]
    [RequireComponent(typeof(Image))]
    public class CurtainView : UIAgent
    {
        private Image _image;

        protected override void OnAwake()
        {
            _image = GetComponent<Image>();
        }

        public Tween DoRadialLerp(float from, float to, float duration)
        {
            _image.type = Image.Type.Filled;
            _image.fillMethod = Image.FillMethod.Radial360;
            _image.fillOrigin = (int)Image.Origin360.Top;
            _image.fillAmount = from;
            return _image.DOFillAmount(to, duration);
        }
    }
}
