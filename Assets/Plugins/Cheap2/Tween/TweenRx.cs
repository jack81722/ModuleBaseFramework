using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheap2.Plugin.TweenExt
{
    public static class TweenRx
    {
        public static IObservable<Tween> ToObservable(this Tween tween)
        {
            return new TweenObservable(tween);
        }
    }
}