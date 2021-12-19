using Cheap2.Plugin.Pool;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example.Drama.Portrait
{   
    [RequireComponent(typeof(Image))]
    public class PortraitAgent : UIAgent
    {
        private Image _image;
        public IPool<PortraitAgent> pool { get; set; }

        protected override void OnAwake()
        {
            _image = GetComponent<Image>();
        }

        public override void Dispose(bool destroyed)
        {
            base.Dispose(destroyed);
            if (destroyed)
                return;
            pool.Push(this);
        }

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = _image.overrideSprite = sprite;
        }
    }
}
