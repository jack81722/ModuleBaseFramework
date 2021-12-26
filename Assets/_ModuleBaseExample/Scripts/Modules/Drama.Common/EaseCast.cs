using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public static class EaseCast 
    {
        public static Ease EaseFromString(this string ease)
        {
            switch (ease)
            {
                case "linear":
                    return Ease.Linear;
                case "in":
                    return Ease.InCubic;
                case "out":
                    return Ease.OutCubic;
                case "inout":
                    return Ease.InOutCubic;
            }
            return Ease.Linear;
        }
    }
}
