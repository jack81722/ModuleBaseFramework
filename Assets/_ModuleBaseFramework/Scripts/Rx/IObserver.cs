using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Rx
{
    public interface IObserver<T>
    {
        void OnError(Exception error);
        void OnComplete(T result);
    }


    public static class ObservableUtils
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onCompleted, Action<Exception> onError)
        {
            var observer = new Subscribe_<T>(onCompleted, onError);
            return source.Subscribe(observer);
        }
    }
}


