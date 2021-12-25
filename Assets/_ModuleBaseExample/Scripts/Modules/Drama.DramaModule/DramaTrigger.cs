using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModuleBased.Example.Drama
{
    [Inject]
    public class DramaTrigger : MonoBehaviour, IPointerClickHandler
    {
        [Inject]
        private IDramaModule _drama;

        public void OnPointerClick(PointerEventData eventData)
        {
            _drama.CompleteOrNextState();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _drama.BeginFastForward();
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                _drama.EndFastForward();
            }
        }
    }
}
