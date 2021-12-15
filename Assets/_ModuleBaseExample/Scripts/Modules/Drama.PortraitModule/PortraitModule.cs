using Cheap2.Plugin.Pool;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Portrait
{
    [Injectable(typeof(IPortraitModule))]
    public class PortraitModule : MonoBehaviour, IPortraitModule
    {
        #region -- Required --
        [Inject]
        private IPool<PortraitAgent> _pool;
        [Inject]
        private PortraitView _view;
        // portrait repo
        #endregion



        private Dictionary<string, PortraitAgent> _portraits;

        private void Awake()
        {
            _portraits = new Dictionary<string, PortraitAgent>();
        }

        private void Start()
        {
            var portrait = NewPortrait("1", "misaki", EPortraitLayout.Middle);
        }

        public PortraitAgent NewPortrait(string id, string src, EPortraitLayout layout)
        {
            var portrait = _pool.Pop();
            _view.PutPortait(portrait, layout);
            portrait.Display();

            return portrait;
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

    }
}
