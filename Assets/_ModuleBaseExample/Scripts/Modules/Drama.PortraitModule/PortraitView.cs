using ModuleBased.ForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Portrait
{
    [Injectable(typeof(PortraitView))]
    public class PortraitView : MonoBehaviour
    {
        [SerializeField]
        RectTransform layout_r;
        [SerializeField]
        RectTransform layout_rm;
        [SerializeField]
        RectTransform layout_m;
        [SerializeField]
        RectTransform layout_lm;
        [SerializeField]
        RectTransform layout_l;

        public void PutPortait(PortraitAgent agent, EPortraitLayout layout)
        {
            Transform trans;
            switch (layout)
            {
                case EPortraitLayout.Right:
                    trans = layout_r;
                    break;
                case EPortraitLayout.MiddleRight:
                    trans = layout_rm;
                    break;
                case EPortraitLayout.Middle:
                    trans = layout_m;
                    break;
                case EPortraitLayout.MiddleLeft:
                    trans = layout_lm;
                    break;
                case EPortraitLayout.Left:
                    trans = layout_l;
                    break;
                default:
                    throw new InvalidOperationException($"invalid layout: {layout}");
            }
            agent.transform.parent = trans;
            agent.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            agent.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        }
    }
}
